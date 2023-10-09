using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using OrderServer.API.Dtos;
using SharedLibrary.Helpers;
using OrderServer.API.Models;
using System.Net.Http.Headers;
using SharedLibrary.Models.Enum;
using SharedLibrary.ResourceFiles;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderServer.API.Services.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;
using AutoMapper.Internal.Mappers;
using OrderServer.API.Mapper;

namespace OrderServer.API.Services.Concrete;

public class OrderServiceForController : IOrderService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<AppDbContext, OrderDelivery> _genericRepositoryForOrder;
	private readonly IGenericRepository<AppDbContext, OutBox> _genericRepositoryForOutBox;
	private readonly IServiceGeneric<OrderDelivery, OrderDto> _serviceGenericForOrder;
	private readonly IServiceGeneric<OutBox, OutBoxDto> _serviceGenericForOutBox;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<OrderServiceForController> _logger;
	private readonly RabbitMQPublisher<OrderDelivery> _rabbitMQPublisher;

	public OrderServiceForController(IGenericRepository<AppDbContext, OrderDelivery> genericRepository, IUnitOfWork unitOfWork, IServiceGeneric<OrderDelivery, OrderDto> serviceGeneric, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OrderServiceForController> logger, RabbitMQPublisher<OrderDelivery> rabbitMQPublisher, IGenericRepository<AppDbContext, OutBox> genericRepositoryForOutBox, IServiceGeneric<OutBox, OutBoxDto> serviceGenericForOutBox)
	{
		_genericRepositoryForOrder = genericRepository;
		_unitOfWork = unitOfWork;
		_serviceGenericForOrder = serviceGeneric;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
		_rabbitMQPublisher = rabbitMQPublisher;
		_genericRepositoryForOutBox = genericRepositoryForOutBox;
		_serviceGenericForOutBox = serviceGenericForOutBox;
	}


	#region User


	public async Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address)
	{
		if (dto == null)
		{
			_logger.LogWarning("Invalid request data");
			return Response<OrderDto>.Fail("Invalid request data", StatusCodes.Status400BadRequest, true);
		}
		try
		{
			var order = new OrderDto()
			{
				Id = Guid.NewGuid(),
				Name = dto.Name,
				CreatedDate = DateTime.Now,
				TotalAmount = dto.TotalAmount,
				Status = OrderStatus.Initial,
				UserId = userId,
				UserName = userName,
				DestinationAddress = address
			};

			var result = await _serviceGenericForOrder.AddAsync(order);

			if (result == null)
			{
				_logger.LogWarning("An error occurred while creating the order");
				return Response<OrderDto>.Fail("An error occurred while creating the order", StatusCodes.Status500InternalServerError, true);
			}

			_logger.LogInformation($"Order created successfully. Order ID: {order.Id}");
			return Response<OrderDto>.Success(order, StatusCodes.Status201Created);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred: {ex.Message}"); // Log the error condition
			return Response<OrderDto>.Fail($"An error occurred: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForUser(string userId)
	{
		try
		{
			var orders = await _serviceGenericForOrder.Where(o => o.UserId == userId);

			if (orders.Data.Count() == 0)
			{
				_logger.LogWarning($"No data found for the provided username: {userId}");
				return Response<IEnumerable<OrderDto>>.Fail("No data found for the provided username", StatusCodes.Status404NotFound, true);
			}

			_logger.LogInformation($"Successfully retrieved orders for user: {userId}");
			return Response<IEnumerable<OrderDto>>.Success(orders.Data, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while fetching orders: {ex.Message}");
			return Response<IEnumerable<OrderDto>>.Fail($"An error occurred while fetching orders: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> UpdateAddressAsync(string userId, string orderId, string address, string authorizationToken)
	{
		try
		{
			var order = (await _serviceGenericForOrder.Where(o => o.UserId == userId && o.Id.ToString() == orderId)).Data.FirstOrDefault();

			if (order == null)
			{
				_logger.LogWarning($"Order not found or does not belong to the user. UserId: {userId}, OrderId: {orderId}");
				return Response<NoDataDto>.Fail("The specified order was not found or does not belong to the user", StatusCodes.Status404NotFound, true);
			}

			// rabbitMq ile yoxlama

			var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer(authorizationToken);

			var updatedOrder = ordersOnTheDeliveryServer.FirstOrDefault(item => item.Id.ToString() == orderId && item.Status != order.Status);

			if (updatedOrder != null)
			{
				order.Status = updatedOrder.Status;
			}

			if (order.Status != OrderStatus.Initial)
			{
				_logger.LogWarning($"Order address can only be updated in the initial status. OrderId: {orderId}, CurrentStatus: {order.Status}");
				return Response<NoDataDto>.Fail("The order's address can only be updated in the initial status", StatusCodes.Status400BadRequest, true);
			}

			order.DestinationAddress = address;
			await _serviceGenericForOrder.UpdateAsync(order, orderId);

			if (order.CourierId != null)
			{
				var orderInOutbox = (await _serviceGenericForOutBox.GetByIdAsync(order.Id.ToString())).Data;

				if (orderInOutbox == null)
				{
					_logger.LogInformation("No matching order found in Outbox table");
					return Response<NoDataDto>.Fail("No matching order found in Outbox table", StatusCodes.Status406NotAcceptable, true);
				}
				orderInOutbox.IsSend = false;
				orderInOutbox.DestinationAddress = order.DestinationAddress;

				await _serviceGenericForOutBox.UpdateAsync(orderInOutbox, orderInOutbox.Id.ToString());

				_logger.LogInformation("Successfully replaced the corresponding order in the Outbox table");
			}

			_logger.LogInformation($"Order address updated successfully. OrderId: {orderId}");
			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while updating the order: {ex.Message}");
			return Response<NoDataDto>.Fail($"An error occurred while updating the order: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> DeleteOrderAsync(string userId, string orderId, string authorizationToken)
	{
		try
		{
			var order = (await _serviceGenericForOrder.Where(o => o.UserId == userId && o.Id.ToString() == orderId)).Data.FirstOrDefault();

			if (order == null)
			{
				_logger.LogWarning($"Order not found or does not belong to the user. UserId: {userId}, OrderId: {orderId}");
				return Response<NoDataDto>.Fail("Order not found or does not belong to the user", StatusCodes.Status404NotFound, true);
			}

			var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer(authorizationToken);
			var updatedOrder = ordersOnTheDeliveryServer.FirstOrDefault(item => item.Id.ToString() == orderId && item.Status != order.Status);

			if (updatedOrder != null)
			{
				order = ObjectMapper.Mapper.Map<OrderDto>(updatedOrder);
			}

			if (order.Status != OrderStatus.Initial)
			{
				_logger.LogWarning($"Order can only be deleted in the initial status. OrderId: {orderId}, CurrentStatus: {order.Status}");
				return Response<NoDataDto>.Fail("The order can only be deleted in the initial status", StatusCodes.Status400BadRequest, true);
			}

			await _serviceGenericForOrder.RemoveAsync(orderId);

			if (order.CourierId != null)
			{
				var orderInOutbox = (await _serviceGenericForOutBox.GetByIdAsync(order.Id.ToString())).Data;

				if (orderInOutbox == null)
				{
					_logger.LogInformation("No matching order found in Outbox table");
					return Response<NoDataDto>.Fail("No matching order found in Outbox table", StatusCodes.Status406NotAcceptable, true);
				}

				orderInOutbox.IsSend = false;
				orderInOutbox.IsDelete = true;
				await _serviceGenericForOutBox.UpdateAsync(orderInOutbox, orderInOutbox.Id.ToString());
				_logger.LogInformation("Successfully replaced the corresponding order in the Outbox table For Delete");
			}

			_logger.LogWarning($"Order deleted successfully. OrderId: {orderId}");
			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, $"An error occurred while deleting the order: {ex.Message}");
			return Response<NoDataDto>.Fail($"An error occurred while deleting the order: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}


	#endregion


	#region Admin


	public async Task<Response<NoDataDto>> ChangeStatusOrder(UpdateStatusDto orderDto, string authorizationToken)
	{
		try
		{
			var order = (await _serviceGenericForOrder.Where(o => o.Id.ToString() == orderDto.OrderId)).Data.FirstOrDefault();

			if (order == null)
			{
				_logger.LogWarning($"Order not found. OrderId: {orderDto.OrderId}");
				return Response<NoDataDto>.Fail("The specified order was not found", StatusCodes.Status404NotFound, true);
			}

			var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer(authorizationToken);
			var updatedOrder = ordersOnTheDeliveryServer.FirstOrDefault(item => item.Id.ToString().ToLower() == orderDto.OrderId.ToLower() && item.Status != order.Status);

			if (updatedOrder != null)
			{
				order = ObjectMapper.Mapper.Map<OrderDto>(updatedOrder);
			}

			order.Status = orderDto.OrderStatus;
			await _serviceGenericForOrder.UpdateAsync(order, orderDto.OrderId);

			if (order.CourierId != null)
			{
				var orderInOutbox = (await _serviceGenericForOutBox.GetByIdAsync(order.Id.ToString())).Data;

				if (orderInOutbox == null)
				{
					_logger.LogInformation("No matching order found in Outbox table");
					return Response<NoDataDto>.Fail("No matching order found in Outbox table", StatusCodes.Status406NotAcceptable, true);
				}

				orderInOutbox.IsSend = false;
				orderInOutbox.Status = order.Status;

				await _serviceGenericForOutBox.UpdateAsync(orderInOutbox, orderDto.OrderId);

				_logger.LogInformation("Successfully replaced the corresponding order in the Outbox table");
			}

			_logger.LogInformation($"Order status updated successfully. OrderId: {orderDto.OrderId}, NewStatus: {orderDto.OrderStatus}");
			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while updating the order: {ex.Message}");
			return Response<NoDataDto>.Fail($"An error occurred while updating the order: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForAdmin()
	{
		try
		{
			var orders = await _genericRepositoryForOrder.GetAllAsync();

			if (orders == null)
			{
				_logger.LogWarning("No order information found in the database");
				return Response<IEnumerable<OrderDto>>.Fail("No order information found in the database", StatusCodes.Status404NotFound, true);
			}

			var orderDtos = orders.Select(o => new OrderDto
			{
				Id = o.Id,
				Name = o.Name,

				Status = o.Status,
				CreatedDate = o.CreatedDate,
				DestinationAddress = o.DestinationAddress,
				TotalAmount = o.TotalAmount,
				UserId = o.UserId,
				UserName = o.UserName,
				CourierId = o.CourierId,
				CourierName = o.CourierName
			}).AsQueryable();

			_logger.LogInformation("Successfully retrieved orders from the database");
			return Response<IEnumerable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving orders: {ex.Message}");
			return Response<IEnumerable<OrderDto>>.Fail($"An error occurred while retrieving orders: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public Response<IEnumerable<CourierWithOrderStatusDto>> GetCourierWithOrderStatus(string courierId)
	{
		try
		{
			var orders = _genericRepositoryForOrder
				.Where(o => o.CourierId == courierId)
				.Select(order => new CourierWithOrderStatusDto
				{
					CourierName = order.CourierName!,
					OrderName = order.Name,
					OrderStatus = order.Status
				}).ToList();

			if (orders == null)
			{
				_logger.LogWarning($"No order information found for courier: {courierId}");
				return Response<IEnumerable<CourierWithOrderStatusDto>>.Fail("No order information found for the specified courier", StatusCodes.Status404NotFound, true);
			}

			_logger.LogInformation($"Successfully retrieved orders for courier: {courierId}");
			return Response<IEnumerable<CourierWithOrderStatusDto>>.Success(orders, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving orders for courier: {courierId}, Error: {ex.Message}");
			return Response<IEnumerable<CourierWithOrderStatusDto>>.Fail($"An error occurred while retrieving orders for courier: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto, string authorizationToken)
	{
		try
		{
			var order = await _genericRepositoryForOrder.Where(x => x.Id.ToString() == dto.OrderId).SingleOrDefaultAsync();

			if (order == null)
			{
				_logger.LogWarning($"Order not found. OrderId: {dto.OrderId}");
				return Response<NoDataDto>.Fail("Order not found", StatusCodes.Status404NotFound, true);
			}

			if (!(order.CourierId == null || order.CourierName == null))
			{
				_logger.LogWarning($"A courier has already been appointed for the order. OrderId: {dto.OrderId}");
				return Response<NoDataDto>.Fail("A courier has already been appointed for the order", StatusCodes.Status400BadRequest, true);
			}

			if (!await CheckUserIsCourier(dto.CourierId!, dto.CourierName!, authorizationToken))
			{
				_logger.LogWarning($"Courier not found. CourierId: {dto.CourierId}, CourierName: {dto.CourierName}");
				return Response<NoDataDto>.Fail("Courier not found", StatusCodes.Status404NotFound, true);
			}

			order.CourierId = dto.CourierId;
			order.CourierName = dto.CourierName;

			// Outbox Table-a yazmaq
			var outbox = new OutBox
			{
				Id = order.Id,
				Name = order.Name,
				UserId = order.UserId,
				UserName = order.UserName,
				DestinationAddress = order.DestinationAddress,
				Status = order.Status,
				CourierName = order.CourierName,
				CourierId = order.CourierId,
				CreatedDate = order.CreatedDate,
				DeliveryDate = order.DeliveryDate,
				TotalAmount = order.TotalAmount,
				IsDelete = false
			};

			_genericRepositoryForOrder.UpdateAsync(order);

			await _genericRepositoryForOutBox.AddAsync(outbox);
			await _unitOfWork.CommitAsync();

			// RabbitMQ  ile datalarin gonderilmesi Delivery Service-e

			_rabbitMQPublisher.Publish(order, OrderDirect.ExchangeName, OrderDirect.QueueName, OrderDirect.RoutingWaterMark);

			_logger.LogInformation($"Order sent to RabbitMQ --> {order.Name}");

			_logger.LogInformation($"Courier assigned to the order successfully. OrderId: {dto.OrderId}, CourierId: {dto.CourierId}, CourierName: {dto.CourierName}");
			return Response<NoDataDto>.Success(StatusCodes.Status201Created);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while assigning a courier to the order: {ex.Message}");
			return Response<NoDataDto>.Fail($"An error occurred while assigning a courier to the order: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}


	#endregion


	#region Courier


	public async Task<Response<IEnumerable<OrderDetailDto>>> ShowOrderDetailAsync(string courierId)
	{
		try
		{
			var orders = await _genericRepositoryForOrder.Where(o => o.CourierId == courierId)
												 .Select(order => new OrderDetailDto
												 {
													 CourierName = order.CourierName!,
													 OrderName = order.Name,
													 OrderStatus = order.Status,
													 CreatedDate = order.CreatedDate,
													 DestinationAddress = order.DestinationAddress,
													 TotalAmount = order.TotalAmount

												 })
												 .ToListAsync();

			if (orders == null)
			{
				_logger.LogWarning($"No orders found for courier: {courierId}");
				return Response<IEnumerable<OrderDetailDto>>.Fail("No orders found for the specified courier", StatusCodes.Status404NotFound, true);
			}

			_logger.BeginScope($"Successfully retrieved orders for courier: {courierId}");
			return Response<IEnumerable<OrderDetailDto>>.Success(orders, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving orders for courier: {courierId}, Error: {ex.Message}");
			return Response<IEnumerable<OrderDetailDto>>.Fail($"Siparişleri gösterirken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}


	#endregion


	#region UserAndCourier


	public async Task<Response<IEnumerable<DeliveryDetailDto>>> ShowDetailDelivery(string userId, string orderId)
	{
		try
		{
			var orders = await _genericRepositoryForOrder.Where(o => (o.UserId == userId || o.CourierId == userId) && o.Id.ToString() == orderId).ToListAsync();

			if (orders.Count == 0)
			{
				_logger.LogWarning($"No orders found for orderId: {orderId} and userId/courierId: {userId}");
				return Response<IEnumerable<DeliveryDetailDto>>.Fail(new ErrorDto("Belirtilen sipariş bulunamadı", true), StatusCodes.Status404NotFound);
			}

			var orderDtos = orders.Select(o => new DeliveryDetailDto
			{
				OrderName = o.Name,
				CreateDate = o.CreatedDate,
				OrderStatus = o.Status,
				CourierName = o.CourierName!,
				DeliveryDate = o.DeliveryDate,
				DestinationAddress = o.DestinationAddress
			}).AsQueryable();

			_logger.LogInformation($"Successfully retrieved orders for orderId: {orderId} and userId/courierId: {userId}");
			return Response<IEnumerable<DeliveryDetailDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving orders: {ex.Message}");
			return Response<IEnumerable<DeliveryDetailDto>>.Fail($"Siparişleri alırken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}



	#endregion


	#region Everyone


	public async Task<Response<IEnumerable<OrderDto>>> GetOrders()
	{
		try
		{
			var orders = await _genericRepositoryForOrder.GetAllAsync();

			if (orders.Count() == 0)
			{
				_logger.LogWarning("No orders found in the database.");
				return Response<IEnumerable<OrderDto>>.Fail("There are no orders in the database", StatusCodes.Status404NotFound, true);
			}

			var orderDtos = orders.Select(o => new OrderDto
			{
				Id = o.Id,
				Name = o.Name,

				Status = o.Status,
				CreatedDate = o.CreatedDate,
				DestinationAddress = o.DestinationAddress,
				TotalAmount = o.TotalAmount,
				UserId = o.UserId,
				UserName = o.UserName,
				CourierId = o.CourierId,
				CourierName = o.CourierName
			}).AsQueryable();

			_logger.LogInformation("Successfully retrieved orders from the database.");
			return Response<IEnumerable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving orders from the database: {ex.Message}");
			return Response<IEnumerable<OrderDto>>.Fail($"An error occurred while retrieving orders from the database: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}


	#endregion


	#region HelperMethod


	public async Task<bool> CheckUserIsCourier(string userId, string userName, string authorizationToken)
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		try
		{
			string identityServiceBaseUrl = _configuration["Microservices:AuthServiceBaseUrl"];
			string checkUserRoleEndpoint = $"{identityServiceBaseUrl}/api/User/GetCourierList";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();

			// Identity Service ile iletişim kuracak HttpClient oluşturun
			using (client)
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);

				// Identity Service'den kullanıcı bilgilerini alın
				var response = await policy.ExecuteAsync(async () =>
				{
					return await client.GetAsync(checkUserRoleEndpoint);
				});

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();

					var userDto = (JsonConvert.DeserializeObject<Response<IEnumerable<ResponseUserDto>>>(responseContent)!);

					var isCourier = userDto.Data.FirstOrDefault(x => x.id == userId && x.userName == userName);

					if (isCourier == null)
					{
						_logger.LogWarning($"User is not a courier. UserId: {userId}, UserName: {userName}");
						return false;
					}

					_logger.LogInformation($"User is a courier. UserId: {userId}, UserName: {userName}");
					return true;
				}
				else
				{
					_logger.LogWarning($"Failed to fetch user data from Identity Service. StatusCode: {response.StatusCode}");
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while checking user role: {ex.Message}");
			return false;
		}
	}

	public async Task<IEnumerable<OrderDelivery>> GetOrdersOnTheDeliveryServer(string authorizationToken)
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		try
		{
			string deliveryServiceBaseUrl = _configuration["Microservices:DeliveryServiceBaseUrl"];
			string getDeliveryOrderEndPoint = $"{deliveryServiceBaseUrl}/api/Delivery/GetDeliveryOrder";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();
			using (client)
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);


				var response = await policy.ExecuteAsync(async () =>
				{
					// Identity Service'den kullanıcı bilgilerini alın
					return await client.GetAsync(getDeliveryOrderEndPoint);
				});

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();

					var orderDeliveryDto = (JsonConvert.DeserializeObject<Response<IEnumerable<OrderDelivery>>>(responseContent)!);

					if (orderDeliveryDto.Data.ToList().Count == 0)
					{
						_logger.LogWarning($"No orders found in the database.");
						return null!;
					}

					_logger.LogInformation($"Successfully retrieved orders from the database.");
					return orderDeliveryDto.Data.ToList();
				}
				else
				{
					_logger.LogWarning($"Failed to fetch user data from Identity Service. StatusCode: {response.StatusCode}");
					return null!;
				}
			}

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Error: {ex.Message}");
			return null!;
		}
	}

	#endregion

}