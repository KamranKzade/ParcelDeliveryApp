using AuthServer.API.Dtos;
using SharedLibrary.Dtos;

namespace AuthServer.API.Services.Abstract;

public interface IUserService
{
	Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
	Task<Response<UserAppDto>> CreateCourierAsync(CreateUserDto createUserDto);
	Task<Response<UserAppDto>> GetUserByNameAsync(string userName);
	Task<Response<NoDataDto>> CreateUserRoles(CreateUserRoleDto dto);
}
