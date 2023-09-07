using AutoMapper;
using OrderService.API.Models;
using OrderService.API.Dtos;
using SharedLibrary.Dtos;

namespace OrderService.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, Order>().ReverseMap();
		CreateMap<NoDataDto, OrderDto>().ReverseMap();
	}
}
