using SharedLibrary.Dtos;
using DeliveryServer.API.Dtos;
using SharedLibrary.Models;

namespace DeliveryServer.API.Services.Abstract;

public interface IDeliveryService
{
	Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId);
	Task<Response<IEnumerable<OrderDelivery>>> GetDeliveryOrder();
}
