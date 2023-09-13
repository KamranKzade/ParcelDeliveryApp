using AutoMapper;
using SharedLibrary.Dtos;
using SharedLibrary.Models;
using OrderService.API.Dtos;

namespace OrderService.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, OrderDelivery>().ReverseMap();
		CreateMap<NoDataDto, OrderDto>().ReverseMap();
	}
}
