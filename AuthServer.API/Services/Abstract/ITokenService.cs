using AuthServer.API.Dtos;
using AuthServer.API.Models;
using AuthServer.API.Configurations;

namespace AuthServer.API.Services.Abstract;

public interface ITokenService
{
	TokenDto CreateToken(UserApp userApp);
	ClientTokenDto CreateTokenByClient(Client client);
}
