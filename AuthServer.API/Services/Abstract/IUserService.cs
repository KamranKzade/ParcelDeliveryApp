using AuthServer.API.Dtos;
using SharedLibrary.Dtos;

namespace AuthServer.API.Services.Abstract;

public interface IUserService
{
	Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
	Task<Response<UserAppDto>> GetUserByNameAsync(string userName);
	Task<Response<NoDataDto>> CreateUserRolesForAdminAsync(string username);
	Task<Response<NoDataDto>> CreateUserRolesForUserAsync(string username);
}
