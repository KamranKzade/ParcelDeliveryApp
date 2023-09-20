using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using OrderServer.API.Dtos;
using OrderServer.API.Models;
using SharedLibrary.Models.Enum;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderServer.API.Services.Abstract;
using SharedLibrary.Repositories.Abstract;

namespace OrderServer.API.Services.Concrete;

public class OrderServiceForController : IOrderService
{
	private readonly AppDbContext _dbContext;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<AppDbContext, OrderDelivery> _genericRepository;
	private readonly IServiceGeneric<OrderDelivery, OrderDto> _serviceGeneric;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<OrderServiceForController> _logger;

	public OrderServiceForController(AppDbContext dbContext, IGenericRepository<AppDbContext, OrderDelivery> genericRepository, IUnitOfWork unitOfWork, IServiceGeneric<OrderDelivery, OrderDto> serviceGeneric, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OrderServiceForController> logger)
	{
		_dbContext = dbContext;
		_genericRepository = genericRepository;
		_unitOfWork = unitOfWork;
		_serviceGeneric = serviceGeneric;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
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

			var result = await _serviceGeneric.AddAsync(order);

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
			var orders = await _dbContext.Orders.Where(o => o.UserId == userId).ToListAsync();

			if (orders.Count == 0)
			{
				_logger.LogWarning($"No data found for the provided username: {userId}");
				return Response<IEnumerable<OrderDto>>.Fail("No data found for the provided username", StatusCodes.Status404NotFound, true);
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

			_logger.LogInformation($"Successfully retrieved orders for user: {userId}");
			return Response<IEnumerable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while fetching orders: {ex.Message}");
			return Response<IEnumerable<OrderDto>>.Fail($"An error occurred while fetching orders: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> UpdateAddressAsync(string userId, string orderId, string address)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Id.ToString() == orderId);

			if (order == null)
			{
				_logger.LogWarning($"Order not found or does not belong to the user. UserId: {userId}, OrderId: {orderId}");
				return Response<NoDataDto>.Fail("The specified order was not found or does not belong to the user", StatusCodes.Status404NotFound, true);
			}

			// rabbitMq ile yoxlama


			var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer();

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
			_genericRepository.UpdateAsync(order);
			await _unitOfWork.CommitAsync();

			_logger.LogInformation($"Order address updated successfully. OrderId: {orderId}");
			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while updating the order: {ex.Message}");
			return Response<NoDataDto>.Fail($"An error occurred while updating the order: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> DeleteOrderAsync(string userId, string orderId)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Id.ToString() == orderId);

			if (order == null)
			{
				_logger.LogWarning($"Order not found or does not belong to the user. UserId: {userId}, OrderId: {orderId}");
				return Response<NoDataDto>.Fail("Order not found or does not belong to the user", StatusCodes.Status404NotFound, true);
			}

			var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer();
			var updatedOrder = ordersOnTheDeliveryServer.FirstOrDefault(item => item.Id.ToString() == orderId && item.Status != order.Status);

			if (updatedOrder != null)
			{
				order = updatedOrder;
			}

			if (order.Status != OrderStatus.Initial)
			{
				_logger.LogWarning($"Order can only be deleted in the initial status. OrderId: {orderId}, CurrentStatus: {order.Status}");
				return Response<NoDataDto>.Fail("The order can only be deleted in the initial status", StatusCodes.Status400BadRequest, true);
			}

			_genericRepository.Remove(order);
			await _unitOfWork.CommitAsync();

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


	public async Task<Response<NoDataDto>> ChangeStatusOrder(UpdateStatusDto orderDto)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == orderDto.OrderId);

			if (order == null)
			{
				_logger.LogWarning($"Order not found. OrderId: {orderDto.OrderId}");
				return Response<NoDataDto>.Fail("The specified order was not found", StatusCodes.Status404NotFound, true);
			}


			//  var ordersOnTheDeliveryServer = await GetOrdersOnTheDeliveryServer();
			// var updatedOrder = ordersOnTheDeliveryServer.FirstOrDefault(item => item.Id.ToString() == orderDto.OrderId && item.Status != order.Status);
			// 
			// if (updatedOrder != null)
			// {
			// 	order = updatedOrder;
			// }

			order.Status = orderDto.OrderStatus;

			_genericRepository.UpdateAsync(order);
			await _unitOfWork.CommitAsync();


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
			var orders = await _dbContext.Orders.ToListAsync();

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
			var orders = _dbContext.Orders
				.Where(o => o.CourierId == courierId)
				.Select(order => new CourierWithOrderStatusDto
				{
					CourierName = order.CourierName,
					OrderName = order.Name,
					OrderStatus = order.Status
				});

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

	public async Task<Response<NoDataDto>> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id.ToString() == dto.OrderId);

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

			if (!await CheckUserIsCourier(dto.CourierId, dto.CourierName))
			{
				_logger.LogWarning($"Courier not found. CourierId: {dto.CourierId}, CourierName: {dto.CourierName}");
				return Response<NoDataDto>.Fail("Courier not found", StatusCodes.Status404NotFound, true);
			}

			order.CourierId = dto.CourierId;
			order.CourierName = dto.CourierName;

			_genericRepository.UpdateAsync(order);
			await _unitOfWork.CommitAsync();

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
			var orders = await _dbContext.Orders.Where(o => o.CourierId == courierId)
												 .Select(order => new OrderDetailDto
												 {
													 CourierName = order.CourierName,
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
			var orders = await _dbContext.Orders.Where(o => (o.UserId == userId || o.CourierId == userId) && o.Id.ToString() == orderId).ToListAsync();

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
				CourierName = o.CourierName,
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


	public async Task<Response<IEnumerable<OrderDto>>> GetOrders()
	{
		try
		{
			var orders = await _dbContext.Orders.ToListAsync();

			if (orders.Count == 0)
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


	public async Task<bool> CheckUserIsCourier(string userId, string userName)
	{
		try
		{
			string identityServiceBaseUrl = _configuration["Microservices:AuthServiceBaseUrl"];
			string checkUserRoleEndpoint = $"{identityServiceBaseUrl}/api/User/GetCourierList";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();

			// Identity Service ile iletişim kuracak HttpClient oluşturun

			// Identity Service'den kullanıcı bilgilerini alın
			var response = await client.GetAsync(checkUserRoleEndpoint);
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
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while checking user role: {ex.Message}");
			return false;
		}

	}


	public async Task<IEnumerable<OrderDelivery>> GetOrdersOnTheDeliveryServer()
	{
		try
		{
			string deliveryServiceBaseUrl = _configuration["Microservices:DeliveryServiceBaseUrl"];
			string getDeliveryOrderEndPoint = $"{deliveryServiceBaseUrl}/api/Delivery/GetDeliveryOrder";

			// IHttpClientFactory'den HttpClient örneği alın
			var client = _httpClientFactory.CreateClient();
			using (client)
			{
				var response = await client.GetAsync(getDeliveryOrderEndPoint);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();

					var orderDeliveryDto = (JsonConvert.DeserializeObject<Response<IEnumerable<OrderDelivery>>>(responseContent)!);

					if (orderDeliveryDto.Data.ToList().Count == 0)
					{
						_logger.LogWarning($"No orders found in the database.");
						return null;
					}

					_logger.LogInformation($"Successfully retrieved orders from the database.");
					return orderDeliveryDto.Data.ToList();
				}
				else
				{
					_logger.LogWarning($"Failed to fetch user data from Identity Service. StatusCode: {response.StatusCode}");
					return null;
				}
			}

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Error: {ex.Message}");
			return null;
		}
	}

}