using AuthServer.API.Configurations;
using AuthServer.API.Dtos;
using AuthServer.API.Models;
using AuthServer.API.Services.Abstract;

namespace AuthServer.API.Services.Concrete;

public class TokenService : ITokenService
{
	public TokenDto CreateToken(UserApp userApp)
	{
		throw new NotImplementedException();
	}

	public ClientTokenDto CreateTokenByClient(Client client)
	{
		throw new NotImplementedException();
	}
}
