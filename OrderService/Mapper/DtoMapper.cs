using AutoMapper;
using OrderService.API.Models;
using OrderService.API.Dtos;

namespace OrderService.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, Order>().ReverseMap();
		CreateMap<CreateOrderDto, OrderDto>().ReverseMap();
	}
}
