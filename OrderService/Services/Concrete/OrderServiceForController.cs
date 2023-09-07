using SharedLibrary.Dtos;
using OrderService.API.Dtos;
using OrderService.API.Models;
using OrderService.API.Mapper;
using OrderService.API.Models.Enum;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Services.Abstract;
using OrderService.API.UnitOfWork.Abstract;
using OrderService.API.Repositories.Abstract;

namespace OrderService.API.Services.Concrete;

public class OrderServiceForController : IOrderService
{
	private readonly AppDbContext _dbContext;
	private readonly IServiceGeneric<Order, OrderDto> _serviceGeneric;
	private readonly IGenericRepository<Order> _genericRepository;
	private readonly IUnitOfWork _unitOfWork;

	public OrderServiceForController(AppDbContext dbContext, IGenericRepository<Order> genericRepository, IUnitOfWork unitOfWork, IServiceGeneric<Order, OrderDto> serviceGeneric)
	{
		_dbContext = dbContext;
		_genericRepository = genericRepository;
		_unitOfWork = unitOfWork;
		_serviceGeneric = serviceGeneric;
	}


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
				return Response<IEnumerable<OrderDto>>.Fail("Bu username'e uygun veri bulunamadı", StatusCodes.Status204NoContent, true);

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

	public Task<Response<OrderDto>> ChangeStatusOrder(OrderDto orderDto)
	{
		throw new NotImplementedException();
	}

	public Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForAdmin()
	{
		throw new NotImplementedException();
	}
}