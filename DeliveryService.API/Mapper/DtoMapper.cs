using AutoMapper;
using SharedLibrary.Models;
using DeliveryServer.API.Dtos;

namespace DeliveryServer.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<GetDeliveryDto, OrderDelivery>().ReverseMap();
		CreateMap<OrderDelivery, ChangeOrderStatusDto>().ReverseMap();
	}
}
