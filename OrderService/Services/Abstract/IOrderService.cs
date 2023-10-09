using SharedLibrary.Dtos;
using OrderServer.API.Dtos;

namespace OrderServer.API.Services.Abstract;

public interface IOrderService
{
	// User
	Task<Response<OrderDto>> CreateOrderAsync(CreateOrderDto dto, string userName, string userId, string address);
	Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForUser(string userId);
	Task<Response<NoDataDto>> UpdateAddressAsync(string userId, string orderName, string address, string authorizationToken);
	Task<Response<NoDataDto>> DeleteOrderAsync(string userId, string orderId, string authorizationToken);
	Task<Response<IEnumerable<DeliveryDetailDto>>> ShowDetailDelivery(string userId, string orderId);

	// Admin
	Task<Response<NoDataDto>> ChangeStatusOrder(UpdateStatusDto orderDto, string authorizationToken);
	Task<Response<IEnumerable<OrderDto>>> GetOrderAsyncForAdmin();
	Response<IEnumerable<CourierWithOrderStatusDto>> GetCourierWithOrderStatus(string courierId);
	//Task<Response<IEnumerable<OrderDto>>> ShowDeliveredOrder(); 
	Task<Response<NoDataDto>> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto, string authorizationToken);


	// Courier
	Task<Response<IEnumerable<OrderDetailDto>>> ShowOrderDetailAsync(string courierId);


	Task<Response<IEnumerable<OrderDto>>> GetOrders();

}
