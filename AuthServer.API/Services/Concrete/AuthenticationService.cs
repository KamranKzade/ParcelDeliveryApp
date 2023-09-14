using SharedLibrary.Dtos;
using AuthServer.API.Dtos;
using AuthServer.API.Models;
using Microsoft.Extensions.Options;
using AuthServer.API.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthServer.API.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using Serilog.Context;
using Serilog;

namespace AuthServer.API.Services.Concrete;

public class AuthenticationService : IAuthenticationService
{
	private readonly List<Client> _client;
	private readonly ITokenService _tokenService;
	private readonly UserManager<UserApp> _userManager;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<UserRefleshToken> _userRefleshTokenService;
	private readonly ILogger<AuthenticationService> _logger;
	public AuthenticationService(IOptions<List<Client>> client, ITokenService tokenService, UserManager<UserApp> userManager,
		IUnitOfWork unitOfWork, IGenericRepository<UserRefleshToken> userRefleshTokenService, ILogger<AuthenticationService> logger)
	{
		_client = client.Value;
		_tokenService = tokenService;
		_userManager = userManager;
		_unitOfWork = unitOfWork;
		_userRefleshTokenService = userRefleshTokenService;
		_logger = logger;
	}

	public async Task<Response<TokenDto>> CreateTokenAsync(LogInDto logIn)
	{
		if (logIn == null)
		{
			throw new ArgumentNullException(nameof(logIn));
		}

		var user = await _userManager.FindByEmailAsync(logIn.Email);

		// Userin olub olmadigini yoxlayiriq
		if (user == null)
		{
			_logger.LogError($"Kullanıcı bulunamadı: {logIn.Email}", logIn.Email);
			return Response<TokenDto>.Fail("Email or Password is wrong", StatusCodes.Status400BadRequest, isShow: true);
		}

		// Parolu yoxlayiriq
		if (!await _userManager.CheckPasswordAsync(user, logIn.Password))
		{
			_logger.LogError($"Parola yanlış girildi: {logIn.Password}", logIn.Email);
			return Response<TokenDto>.Fail("Email or Password is wrong", StatusCodes.Status400BadRequest, isShow: true);
		}

		var token = _tokenService.CreateToken(user);
		var userRefleshToken = await _userRefleshTokenService.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

		if (userRefleshToken == null)
		{
			await _userRefleshTokenService.AddAsync(new UserRefleshToken
			{
				UserId = user.Id,
				RefleshToken = token.RefleshToken,
				Expiration = token.RefleshTokenExpiration
			});
		}
		else
		{
			userRefleshToken.RefleshToken = token.RefleshToken;
			userRefleshToken.Expiration = token.RefleshTokenExpiration;
		}

		await _unitOfWork.CommitAsync();
		
		Log.Information($"Token oluşturuldu. Kullanıcı: {user.Name}");
		return Response<TokenDto>.Success(token, StatusCodes.Status200OK);
	}


	public Response<ClientTokenDto> CreateTokenByClientAsync(ClientLogInDto clientLogInDto)
	{
		var client = _client.SingleOrDefault(x => x.Id == clientLogInDto.ClientId && x.Secret == clientLogInDto.ClientSecret);

		if (client == null)
		{
			return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", StatusCodes.Status404NotFound, true);
		}

		var token = _tokenService.CreateTokenByClient(client);
		return Response<ClientTokenDto>.Success(token, StatusCodes.Status200OK);
	}

	public async Task<Response<TokenDto>> CreateTokenByRefleshTokenAsync(string refleshToken)
	{
		// Movcud refleshTokeni aliriq
		var existRefleshToken = await _userRefleshTokenService.Where(x => x.RefleshToken == refleshToken).SingleOrDefaultAsync();

		if (existRefleshToken == null)
		{
			return Response<TokenDto>.Fail("Reflesh token not found", StatusCodes.Status404NotFound, true);
		}

		// Useri tapiram UserId-e gore
		var user = await _userManager.FindByIdAsync(existRefleshToken.UserId);

		if (user == null)
		{
			return Response<TokenDto>.Fail("User Id not found", StatusCodes.Status404NotFound, true);
		}

		// Yeni token yaradib
		var tokenDto = _tokenService.CreateToken(user);

		// movcud tokenin refleshtokeni ile vaxtini, yeni yaradilan tokene uygun olaraq deyisirik
		existRefleshToken.RefleshToken = tokenDto.RefleshToken;
		existRefleshToken.Expiration = tokenDto.RefleshTokenExpiration;

		await _unitOfWork.CommitAsync();

		return Response<TokenDto>.Success(tokenDto, StatusCodes.Status200OK);
	}

	public async Task<Response<NoDataDto>> RevokeRefleshTokenAsync(string refleshToken)
	{
		var existRefleshToken = await _userRefleshTokenService.Where(x => x.RefleshToken == refleshToken).SingleOrDefaultAsync();

		if (existRefleshToken == null)
		{
			return Response<NoDataDto>.Fail("Reflesh token not found ", 404, true);
		}

		_userRefleshTokenService.Remove(existRefleshToken);

		await _unitOfWork.CommitAsync();

		return Response<NoDataDto>.Success(StatusCodes.Status200OK);

	}
}
