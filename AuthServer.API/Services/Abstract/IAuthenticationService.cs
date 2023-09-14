using SharedLibrary.Dtos;
using AuthServer.API.Dtos;

namespace AuthServer.API.Services.Abstract;

public interface IAuthenticationService
{
	 Task<Response<TokenDto>> CreateTokenAsync(LogInDto logIn);
	 Task<Response<TokenDto>> CreateTokenByRefleshTokenAsync(string refleshToken);
	 Task<Response<NoDataDto>> RevokeRefleshTokenAsync(string refleshToken);
	 Response<ClientTokenDto> CreateTokenByClientAsync(ClientLogInDto clientLogInDto);
}
