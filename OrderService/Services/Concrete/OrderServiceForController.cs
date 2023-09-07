using SharedLibrary.Dtos;
using OrderService.API.Dtos;
using OrderService.API.Models;
using OrderService.API.Models.Enum;
using OrderService.API.UnitOfWork.Abstract;
using OrderService.API.Services.Abstract;
using OrderService.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using OrderService.API.Mapper;

namespace OrderService.API.Services.Concrete;

public class OrderServiceForController : IOrderService
{
	private readonly AppDbContext _dbContext;
	private readonly IGenericRepository<Order> _genericRepository;
	private readonly IUnitOfWork _unitOfWork;

	public OrderServiceForController(AppDbContext dbContext, IGenericRepository<Order> genericRepository, IUnitOfWork unitOfWork)
	{
		_dbContext = dbContext;
		_genericRepository = genericRepository;
		_unitOfWork = unitOfWork;
	}

	public Task<Response<OrderDto>> ChangeStatusOrder(OrderDto orderDto)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address)
	{
		if (dto == null) throw new ArgumentNullException(nameof(dto));

		var order = new Order()
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


		await _dbContext.AddAsync(order);
		await _unitOfWork.CommitAsync();

		return Response<OrderDto>.Success(ObjectMapper.Mapper.Map<OrderDto>(order), StatusCodes.Status201Created);
	}

	public Task<Response<NoDataDto>> DeleteOrderAsync(string orderId)
	{
		throw new NotImplementedException();
	}

	public Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForAdmin()
	{
		throw new NotImplementedException();
	}

	public async Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForUser(string userId)
	{
		var orders = await _dbContext.Orders.Where(o => o.UserId == userId).ToListAsync();

		if (orders.Count == 0)
		{
			return Response<IQueryable<OrderDto>>.Fail("Bu username'e uygun veri bulunamadı", StatusCodes.Status204NoContent, true);
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

			// ObjectMapper.Mapper.Map<OrderDto>(o)

		}).AsQueryable();

		return Response<IQueryable<OrderDto>>.Success(orderDtos, StatusCodes.Status200OK);
	}

	public Task<Response<OrderDto>> UpdateAddressAsync(string address)
	{
		throw new NotImplementedException();
	}
}