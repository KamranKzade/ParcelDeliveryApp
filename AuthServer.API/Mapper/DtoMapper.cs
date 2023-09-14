using AutoMapper;
using AuthServer.API.Dtos;
using AuthServer.API.Models;

namespace AuthServer.API.Mapper;

public class DtoMapper : Profile
{
	public DtoMapper()
	{
		CreateMap<UserAppDto, UserApp>().ReverseMap();
	}
}