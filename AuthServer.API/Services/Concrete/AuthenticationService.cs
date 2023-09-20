using Serilog.Context;
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
using SharedLibrary.Models;
using Serilog.Events;

namespace AuthServer.API.Services.Concrete;

public class AuthenticationService : IAuthenticationService
{
	private readonly List<Client> _client;
	private readonly ITokenService _tokenService;
	private readonly UserManager<UserApp> _userManager;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IGenericRepository<AppDbContext, UserRefleshToken> _userRefleshTokenService;
	private readonly IGenericRepository<AppDbContext, LogEntry> _logs;
	private readonly ILogger<AuthenticationService> _logger;

	public AuthenticationService(IOptions<List<Client>> client, ITokenService tokenService, UserManager<UserApp> userManager,
		IUnitOfWork unitOfWork, IGenericRepository<AppDbContext, UserRefleshToken> userRefleshTokenService, ILogger<AuthenticationService> logger, IGenericRepository<AppDbContext, LogEntry> logs)
	{
		_client = client.Value;
		_tokenService = tokenService;
		_userManager = userManager;
		_unitOfWork = unitOfWork;
		_userRefleshTokenService = userRefleshTokenService;
		_logger = logger;
		_logs = logs;
	}

	public async Task<Response<TokenDto>> CreateTokenAsync(LogInDto logIn)
	{
		if (logIn == null)
		{
			throw new ArgumentNullException(nameof(logIn));
		}

		try
		{
			var user = await _userManager.FindByEmailAsync(logIn.Email);

			// Userin olub olmadigini yoxlayiriq
			if (user == null)
			{
				_logger.LogWarning($"User not found: {logIn.Email}", logIn.Email);
				return Response<TokenDto>.Fail("Email or Password is wrong", StatusCodes.Status400BadRequest, isShow: true);
			}

			// Parolu yoxlayiriq
			if (!await _userManager.CheckPasswordAsync(user, logIn.Password))
			{
				_logger.LogWarning($"The password was entered incorrectly: {logIn.Password}", logIn.Email);
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


			_logger.LogInformation($"Token created. User: {user.Name}");

			// var log = new LogEntry
			// {
			// 	Level = LogLevel.Information.ToString(),
			// 	MessageTemplate = $"Token created. User: {user.Name}",
			// 	Exception = null,
			// 	Timestamp = DateTime.UtcNow,
			// 	Properties = $"Token created. User: {user.Name}"
			// };
			// 
			// _logs.AddAsync(log);
			// await _unitOfWork.CommitAsync();

			return Response<TokenDto>.Success(token, StatusCodes.Status200OK);

		}
		catch (Exception ex)
		{
			using (LogContext.PushProperty("LogEvent", ex.ToString()))
				_logger.LogError(ex.ToString(), "An error occurred while creating the token");

			return Response<TokenDto>.Fail("An error occurred while creating the token", StatusCodes.Status500InternalServerError, isShow: true);
		}
	}

	public Response<ClientTokenDto> CreateTokenByClientAsync(ClientLogInDto clientLogInDto)
	{
		try
		{
			var client = _client.SingleOrDefault(x => x.Id == clientLogInDto.ClientId && x.Secret == clientLogInDto.ClientSecret);

			if (client == null)
			{
				_logger.LogWarning("ClientId or ClientSecret not found");
				return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", StatusCodes.Status404NotFound, true);
			}

			var token = _tokenService.CreateTokenByClient(client);
			_logger.LogInformation("The token was created successfully");
			return Response<ClientTokenDto>.Success(token, StatusCodes.Status200OK);

		}
		catch (Exception ex)
		{
			using (LogContext.PushProperty("LogEvent", ex.ToString()))
				_logger.LogError(ex.ToString(), "An error occurred while creating the token");

			return Response<ClientTokenDto>.Fail("An error occurred while creating the token", StatusCodes.Status500InternalServerError, isShow: true);
		}
	}

	public async Task<Response<TokenDto>> CreateTokenByRefleshTokenAsync(string refleshToken)
	{
		try
		{
			// Movcud refleshTokeni aliriq
			var existRefleshToken = await _userRefleshTokenService.Where(x => x.RefleshToken == refleshToken).SingleOrDefaultAsync();

			if (existRefleshToken == null)
			{
				_logger.LogWarning("Reflesh token not found");
				return Response<TokenDto>.Fail("Reflesh token not found", StatusCodes.Status404NotFound, true);
			}

			// Useri tapiram UserId-e gore
			var user = await _userManager.FindByIdAsync(existRefleshToken.UserId);

			if (user == null)
			{
				_logger.LogWarning("User Id not found");
				return Response<TokenDto>.Fail("User Id not found", StatusCodes.Status404NotFound, true);
			}

			// Yeni token yaradib
			var tokenDto = _tokenService.CreateToken(user);

			// movcud tokenin refleshtokeni ile vaxtini, yeni yaradilan tokene uygun olaraq deyisirik
			existRefleshToken.RefleshToken = tokenDto.RefleshToken;
			existRefleshToken.Expiration = tokenDto.RefleshTokenExpiration;

			await _unitOfWork.CommitAsync();

			_logger.LogInformation("The token was successfully created as a refresh token");
			return Response<TokenDto>.Success(tokenDto, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			using (LogContext.PushProperty("LogEvent", ex.ToString()))
				_logger.LogError(ex.ToString(), "An error occurred while creating the token");

			return Response<TokenDto>.Fail("An error occurred while creating the token", StatusCodes.Status500InternalServerError, isShow: true);
		}

	}

	public async Task<Response<NoDataDto>> RevokeRefleshTokenAsync(string refleshToken)
	{
		try
		{
			var existRefleshToken = await _userRefleshTokenService.Where(x => x.RefleshToken == refleshToken).SingleOrDefaultAsync();

			if (existRefleshToken == null)
			{
				_logger.LogWarning("Reflesh token not found");
				return Response<NoDataDto>.Fail("Reflesh token not found", StatusCodes.Status404NotFound, true);
			}

			_userRefleshTokenService.Remove(existRefleshToken);

			await _unitOfWork.CommitAsync();

			_logger.LogInformation("The reflash token was successfully deleted");
			return Response<NoDataDto>.Success(StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			using (LogContext.PushProperty("LogEvent", ex.ToString()))
				_logger.LogError(ex.ToString(), "An error occurred while creating the token");

			return Response<NoDataDto>.Fail("An error occurred while creating the token", StatusCodes.Status500InternalServerError, isShow: true);
		}
	}
}
