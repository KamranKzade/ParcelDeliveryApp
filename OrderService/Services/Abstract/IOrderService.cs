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
	Task<Response<IEnumerable<DeliveryDetailDto>>> ShowDetailDelivery(string userId, string orderId);

	// Admin
	Task<Response<NoDataDto>> ChangeStatusOrder(UpdateStatusDto orderDto);
	Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForAdmin();
	Response<IEnumerable<CourierWithOrderStatusDto>> GetCourierWithOrderStatus(string courierId);
	//Task<Response<IEnumerable<OrderDto>>> ShowDeliveredOrder(); 
	Task<Response<NoDataDto>> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto);


	// Courier
	Task<Response<IEnumerable<OrderDetailDto>>> ShowOrderDetailAsync(string courierId);


	Task<Response<IEnumerable<OrderDto>>> GetOrders();

}
