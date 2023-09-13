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
		CreateMap<NoDataDto, OrderDto>().ReverseMap();
	}
}
