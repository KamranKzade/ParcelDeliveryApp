using AutoMapper;
using SharedLibrary.Dtos;
using OrderService.API.Dtos;
using OrderService.API.Models;

namespace OrderService.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, Order>().ReverseMap();
		CreateMap<NoDataDto, OrderDto>().ReverseMap();
	}
}
