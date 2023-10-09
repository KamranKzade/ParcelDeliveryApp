using AutoMapper;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using OrderServer.API.Dtos;

namespace OrderServer.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, OrderDelivery>().ReverseMap();
		CreateMap<OrderDto, OutBoxDto>().ReverseMap();
		CreateMap<OutBox, OutBoxDto>().ReverseMap();
		CreateMap<NoDataDto, OrderDto>().ReverseMap();
		CreateMap<CourierWithOrderStatusDto, OrderDto>().ReverseMap();
		CreateMap<IEnumerable<CourierWithOrderStatusDto>, IEnumerable<OrderDto>>().ReverseMap();
	}
}
