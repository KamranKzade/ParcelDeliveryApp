using OrderService.Dtos;
using SharedLibrary.Dtos;
using OrderService.Models;
using OrderService.Models.Enum;
using OrderService.Services.Abstract;
using OrderService.UnitOfWork.Abstract;
using OrderService.Repositories.Abstract;

namespace OrderService.Services.Concrete;

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

	public async Task<Response<Order>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address)
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

		return Response<Order>.Success(order, StatusCodes.Status201Created);
	}

	public Task<Response<NoDataDto>> DeleteOrderAsync(string orderId)
	{
		throw new NotImplementedException();
	}

	public Task<Response<OrderDto>> GetOrderAsyncForAdmin()
	{
		throw new NotImplementedException();
	}

	public Task<Response<OrderDto>> GetOrderAsyncForUser()
	{
		throw new NotImplementedException();
	}

	public Task<Response<OrderDto>> UpdateAddressAsync(string address)
	{
		throw new NotImplementedException();
	}
}