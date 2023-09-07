using SharedLibrary.Dtos;
using OrderService.API.Dtos;
using OrderService.API.Models;

namespace OrderService.API.Services.Abstract;

public interface IOrderService
{
	// User
	Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address);
	Task<Response<OrderDto>> UpdateAddressAsync(string address);
	Task<Response<NoDataDto>> DeleteOrderAsync(string orderId);
	Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForUser(string userId);


	// Admin
	Task<Response<OrderDto>> ChangeStatusOrder(OrderDto orderDto);
	Task<Response<IQueryable<OrderDto>>> GetOrderAsyncForAdmin();
}
