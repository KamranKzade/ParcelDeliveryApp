using AutoMapper;
using OrderService.Dtos;
using OrderService.Models;

namespace OrderService.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<OrderDto, Order>().ReverseMap();
	}
}
