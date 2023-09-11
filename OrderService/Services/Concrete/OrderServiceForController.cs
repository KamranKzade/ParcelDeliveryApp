using Newtonsoft.Json;
using SharedLibrary.Dtos;
using OrderService.API.Dtos;
using OrderService.API.Models;
using OrderService.API.Models.Enum;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderService.API.Services.Abstract;
using SharedLibrary.Repositories.Abstract;

namespace OrderService.API.Services.Concrete;

public class OrderServiceForController : IOrderService
{
	private readonly AppDbContext _dbContext;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<Order> _genericRepository;
	private readonly IServiceGeneric<Order, OrderDto> _serviceGeneric;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;

	public OrderServiceForController(AppDbContext dbContext, IGenericRepository<Order> genericRepository, IUnitOfWork unitOfWork, IServiceGeneric<Order, OrderDto> serviceGeneric, IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		_dbContext = dbContext;
		_genericRepository = genericRepository;
		_unitOfWork = unitOfWork;
		_serviceGeneric = serviceGeneric;
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
	}


	#region User


	public async Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address)
	{
		if (dto == null) return Response<OrderDto>.Fail("Geçersiz istek verisi", StatusCodes.Status400BadRequest, true);

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
				return Response<OrderDto>.Fail("Sipariş oluşturulurken bir hata oluştu", StatusCodes.Status500InternalServerError, true);
			}
			return Response<OrderDto>.Success(order, StatusCodes.Status201Created);

		}
		catch (Exception ex)
		{
			return Response<OrderDto>.Fail($"Bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);

		}
	}

	public async Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForUser(string userId)
	{
		try
		{
			var orders = await _dbContext.Orders.Where(o => o.UserId == userId).ToListAsync();

			if (orders.Count == 0)
				return Response<IEnumerable<OrderDto>>.Fail("Bu username'e uygun veri bulunamadı", StatusCodes.Status404NotFound, true);

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

			return Response<IEnumerable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			return Response<IEnumerable<OrderDto>>.Fail($"Siparişleri alırken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> UpdateAddressAsync(string userId, string orderId, string address)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Id.ToString() == orderId);

			if (order == null)
			{
				return Response<NoDataDto>.Fail("Belirtilen sipariş bulunamadı veya kullanıcıya ait değil", StatusCodes.Status404NotFound, true);
			}

			if (order.Status != OrderStatus.Initial)
			{
				return Response<NoDataDto>.Fail("Siparişin adresi yalnızca başlangıç ​​durumundayken güncellenebilir", StatusCodes.Status400BadRequest, true);
			}

			order.DestinationAddress = address;
			_genericRepository.UpdateAsync(order);
			await _unitOfWork.CommitAsync();

			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			// Hata yakalama ve uygun bir hata mesajı döndürme
			return Response<NoDataDto>.Fail($"Sipariş güncellenirken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<NoDataDto>> DeleteOrderAsync(string userId, string orderId)
	{
		try
		{
			var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Id.ToString() == orderId);

			if (order == null)
			{
				return Response<NoDataDto>.Fail("Belirtilen sipariş bulunamadı veya kullanıcıya ait değil", StatusCodes.Status404NotFound, true);
			}

			// RabbitMq muraciet

			if (order.Status != OrderStatus.Initial)
			{
				return Response<NoDataDto>.Fail("Siparişin yalnızca başlangıç durumundayken silinebilir", StatusCodes.Status400BadRequest, true);
			}

			_genericRepository.Remove(order);
			await _unitOfWork.CommitAsync();

			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			return Response<NoDataDto>.Fail($"Sipariş silinirken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
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
				return Response<NoDataDto>.Fail("Belirtilen sipariş bulunamadı", StatusCodes.Status404NotFound, true);
			}

			order.Status = orderDto.OrderStatus;

			_genericRepository.UpdateAsync(order);
			await _unitOfWork.CommitAsync();


			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			// Hata yakalama ve uygun bir hata mesajı döndürme
			return Response<NoDataDto>.Fail($"Sipariş güncellenirken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}

	public async Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForAdmin()
	{
		try
		{
			var orders = await _dbContext.Orders.ToListAsync();

			if (orders == null)
			{
				return Response<IEnumerable<OrderDto>>.Fail("Veritabanında sipariş bilgisi bulunamadı", StatusCodes.Status404NotFound, true);
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

			return Response<IEnumerable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			return Response<IEnumerable<OrderDto>>.Fail($"Sipariş güncellenirken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}


	}

	public Response<IEnumerable<CourierWithOrderStatusDto>> GetCourierWithOrderStatus(string courierId)
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
			return Response<IEnumerable<CourierWithOrderStatusDto>>.Fail("Veritabanında sipariş bilgisi bulunamadı", StatusCodes.Status404NotFound, true);
		}

		return Response<IEnumerable<CourierWithOrderStatusDto>>.Success(orders, StatusCodes.Status200OK);
	}

	public async Task<Response<NoDataDto>> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto)
	{
		var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id.ToString() == dto.OrderId);

		if (order == null)
		{
			return Response<NoDataDto>.Fail("Order tapilmadi", StatusCodes.Status404NotFound, true);
		}

		if (!(order.CourierId == null || order.CourierName == null))
		{
			return Response<NoDataDto>.Fail("A courier has already been appointed for the order", StatusCodes.Status404NotFound, true);
		}

		if (!await CheckUserIsCourier(dto.CourierId, dto.CourierName))
		{
			return Response<NoDataDto>.Fail("Courirer not founded", StatusCodes.Status404NotFound, true);
		}

		order.CourierId = dto.CourierId;
		order.CourierName = dto.CourierName;

		_genericRepository.UpdateAsync(order);
		await _unitOfWork.CommitAsync();

		return Response<NoDataDto>.Success(StatusCodes.Status201Created);
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
				return Response<IEnumerable<OrderDetailDto>>.Fail("Kuryere uygun order tapilmadi", StatusCodes.Status404NotFound, true);

			return Response<IEnumerable<OrderDetailDto>>.Success(orders, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
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

			return Response<IEnumerable<DeliveryDetailDto>>.Success(orderDtos, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			return Response<IEnumerable<DeliveryDetailDto>>.Fail($"Siparişleri alırken bir hata oluştu: {ex.Message}", StatusCodes.Status500InternalServerError, true);
		}
	}



	#endregion


	public async Task<bool> CheckUserIsCourier(string userId, string userName)
	{
		string identityServiceBaseUrl = _configuration["AuthServiceBaseUrl"];
		string checkUserRoleEndpoint = $"{identityServiceBaseUrl}/api/User/GetCourierList";

		// IHttpClientFactory'den HttpClient örneği alın
		var client = _httpClientFactory.CreateClient();

		// Identity Service ile iletişim kuracak HttpClient oluşturun
		using (client)
		{
			// Identity Service'den kullanıcı bilgilerini alın
			var response = await client.GetAsync(checkUserRoleEndpoint);
			if (response.IsSuccessStatusCode)
			{
				var responseContent = await response.Content.ReadAsStringAsync();

				var userDto = (JsonConvert.DeserializeObject<Response<IEnumerable<ResponseUserDto>>>(responseContent)!);

				var isCourier = userDto.Data.FirstOrDefault(x => x.id == userId && x.userName == userName);

				if (isCourier == null)
				{
					return false;
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}

}