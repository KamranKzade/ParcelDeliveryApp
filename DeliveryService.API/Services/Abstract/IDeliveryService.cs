using SharedLibrary.Dtos;
using SharedLibrary.Models;
using DeliveryServer.API.Dtos;

namespace DeliveryServer.API.Services.Abstract;

public interface IDeliveryService
{
	Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId);
	Task<Response<IEnumerable<OrderDelivery>>> GetDeliveryOrder();
}
