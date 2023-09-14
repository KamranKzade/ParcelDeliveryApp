using SharedLibrary.Dtos;
using AuthServer.API.Dtos;
using AuthServer.API.Models;
using AuthServer.API.Mapper;
using Microsoft.AspNetCore.Identity;
using AuthServer.API.Services.Abstract;

namespace AuthServer.API.Services.Concrete;

public class UserService : IUserService
{
	private readonly UserManager<UserApp> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ILogger<UserService> _logger;

	public UserService(UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserService> logger)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_logger = logger;
	}

	public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
	{
		try
		{
			var user = new UserApp
			{
				Email = createUserDto.Email,
				UserName = createUserDto.UserName,
				Name = createUserDto.Name,
				Surname = createUserDto.FirstName,
				Address = createUserDto.Address,
				PostalCode = createUserDto.PostalCode,
				Birhtdate = Convert.ToDateTime(createUserDto.Birhtdate),
			};

			var result = await _userManager.CreateAsync(user, createUserDto.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(x => x.Description).ToList();
				_logger.LogError($"Failed to create user: {createUserDto.Email}\nErrors: {string.Join(", ", errors)}");
				return Response<UserAppDto>.Fail(new ErrorDto(errors, true), StatusCodes.Status400BadRequest);
			}
			_logger.LogInformation($"User created successfully: {createUserDto.Email}");
			return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while creating a user: {createUserDto.Email}", createUserDto.Email);
			throw;
		}
	}

	public async Task<Response<UserAppDto>> GetUserByNameAsync(string userName)
	{
		try
		{
			var user = await _userManager.FindByNameAsync(userName);

			if (user == null)
			{
				_logger.LogInformation($"User with UserName '{userName}' not found");
				return Response<UserAppDto>.Fail("UserName not found", StatusCodes.Status404NotFound, true);
			}
			_logger.LogInformation($"User retrieved successfully by UserName: {userName}");
			return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while retrieving user by UserName: {userName}", userName);
			throw;
		}
	}

	public async Task<Response<IEnumerable<UserAppDto>>> GetCourierList()
	{
		try
		{
			var user = await _userManager.GetUsersInRoleAsync("Courier");

			if (user == null)
			{
				_logger.LogInformation("No users found in the 'Courier' role");
				return Response<IEnumerable<UserAppDto>>.Fail("UserName not found", StatusCodes.Status404NotFound, true);
			}

			var userdto = user.Select(o => new UserAppDto
			{
				Id = o.Id,
				UserName = o.Name,
				Email = o.Email
			}).AsQueryable();

			_logger.LogInformation("List of courier users retrieved successfully");
			return Response<IEnumerable<UserAppDto>>.Success(userdto, StatusCodes.Status200OK);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while retrieving the list of couriers");
			throw;
		}
	}


	public async Task<Response<NoDataDto>> CreateUserRoles(CreateUserRoleDto dto)
	{
		try
		{
			if (!await _roleManager.RoleExistsAsync(dto.RoleName))
			{
				await _roleManager.CreateAsync(new IdentityRole { Name = dto.RoleName.ToLower() });
				_logger.LogInformation($"Role '{dto.RoleName}' created successfully.");
			}

			var user = await _userManager.FindByNameAsync(dto.UserName);

			if (user == null)
			{
				_logger.LogInformation($"User with UserName '{dto.UserName}' not found.");
				return Response<NoDataDto>.Fail("UserName not found", StatusCodes.Status404NotFound, true);
			}

			await _userManager.AddToRoleAsync(user, dto.RoleName.ToLower());
			_logger.LogInformation($"User '{user.UserName}' assigned to role '{dto.RoleName}' successfully.");

			return Response<NoDataDto>.Success(StatusCodes.Status201Created);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while creating user roles");
			throw;
		}
	}

	public async Task<Response<UserAppDto>> CreateCourierAsync(CreateUserDto createUserDto)
	{
		try
		{
			var user = new UserApp
			{
				Email = createUserDto.Email,
				UserName = createUserDto.UserName,
				Name = createUserDto.Name,
				Surname = createUserDto.FirstName,
				Address = createUserDto.Address,
				PostalCode = createUserDto.PostalCode,
				Birhtdate = Convert.ToDateTime(createUserDto.Birhtdate),
			};

			var result = await _userManager.CreateAsync(user, createUserDto.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(x => x.Description).ToList();
				_logger.LogError($"Failed to create a courier user: {string.Join(", ", errors)}");
				return Response<UserAppDto>.Fail(new ErrorDto(errors, true), StatusCodes.Status400BadRequest);
			}

			if (!await _roleManager.RoleExistsAsync("Courier"))
			{
				await _roleManager.CreateAsync(new IdentityRole { Name = "Courier" });
				_logger.LogInformation("Role 'Courier' created successfully.");
			}

			await _userManager.AddToRoleAsync(user, "Courier");
			_logger.LogInformation($"User '{user.UserName}' assigned to role 'Courier' successfully.");
			return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), StatusCodes.Status200OK);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while creating a courier user");
			throw;
		}
	}
}
