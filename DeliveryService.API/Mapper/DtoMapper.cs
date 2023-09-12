using AutoMapper;
using DeliveryServer.API.Dtos;
using DeliveryServer.API.Models;

namespace DeliveryServer.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<ChangeOrderStatusDto, OrderDelivery>().ReverseMap();
	}
}
