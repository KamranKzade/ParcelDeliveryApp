using SharedLibrary.Dtos;
using OrderService.API.Dtos;

namespace OrderService.API.Services.Abstract;

public interface IOrderService
{
	// User
	Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address);
	Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForUser(string userId);
	Task<Response<NoDataDto>> UpdateAddressAsync(string userId, string orderName, string address);


	Task<Response<NoDataDto>> DeleteOrderAsync(string userId, string orderId);


	// Admin
	Task<Response<OrderDto>> ChangeStatusOrder(OrderDto orderDto);
	Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForAdmin();
}
