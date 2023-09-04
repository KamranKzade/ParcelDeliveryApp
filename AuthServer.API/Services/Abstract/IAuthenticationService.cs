using AuthServer.API.Dtos;
using SharedLibrary.Dtos;

namespace AuthServer.API.Services.Abstract;

public interface IAuthenticationService
{
	 Task<Response<TokenDto>> CreateTokenAsync(LogInDto logIn);
	 Task<Response<TokenDto>> CreateTokenByRefleshTokenAsync(string refleshToken);
	 Task<Response<NoDataDto>> RevokeRefleshTokenAsync(string refleshToken);
	 Response<ClientTokenDto> CreateTokenByClientAsync(ClientLogInDto clientLogInDto);
}
