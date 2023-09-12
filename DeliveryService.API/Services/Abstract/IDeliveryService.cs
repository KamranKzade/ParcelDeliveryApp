using DeliveryServer.API.Dtos;
using SharedLibrary.Dtos;

namespace DeliveryServer.API.Services.Abstract;

public interface IDeliveryService
{
	Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId);
}
