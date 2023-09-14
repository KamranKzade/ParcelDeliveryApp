using SharedLibrary.Dtos;
using DeliveryServer.API.Dtos;

namespace DeliveryServer.API.Services.Abstract;

public interface IDeliveryService
{
	Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId);
}
