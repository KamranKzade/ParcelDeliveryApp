using SharedLibrary.Dtos;
using DeliveryServer.API.Dtos;
using DeliveryServer.API.Services.Abstract;

namespace DeliveryServer.API.Services.Concrete;

public class DeliveryService : IDeliveryService
{
	public Task<Response<NoDataDto>> ChangeOrderStatus(ChangeOrderStatusDto dto, string courierId)
	{
		throw new NotImplementedException();
	}
}
