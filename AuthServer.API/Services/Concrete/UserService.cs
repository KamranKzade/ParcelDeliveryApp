using SharedLibrary.Dtos;
using AuthServer.API.Dtos;
using AuthServer.API.Models;
using Microsoft.AspNetCore.Identity;
using AuthServer.API.Services.Abstract;
using AutoMapper.Internal.Mappers;
using AuthServer.API.Mapper;

namespace AuthServer.API.Services.Concrete;

public class UserService : IUserService
{
	private readonly UserManager<UserApp> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;

	public UserService(UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager)
	{
		_userManager = userManager;
		_roleManager = roleManager;
	}

	public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
	{
		var user = new UserApp
		{
			Email = createUserDto.Email,
			UserName = createUserDto.UserName,
		};

		var result = await _userManager.CreateAsync(user, createUserDto.Password);

		if (!result.Succeeded)
		{
			var errors = result.Errors.Select(x => x.Description).ToList();

			return Response<UserAppDto>.Fail(new ErrorDto(errors, true), StatusCodes.Status400BadRequest);
		}
		return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), StatusCodes.Status200OK);
	}


	// Usere admin rolunun elave edilmesi
	public async Task<Response<NoDataDto>> CreateUserRoles(string username)
	{
		if (!await _roleManager.RoleExistsAsync("admin"))
		{
			await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
		}

		var user = await _userManager.FindByNameAsync(username);

		await _userManager.AddToRoleAsync(user, "admin");

		return Response<NoDataDto>.Success(StatusCodes.Status201Created);
	}

	public async Task<Response<UserAppDto>> GetUserByNameAsync(string userName)
	{
		var user = await _userManager.FindByNameAsync(userName);

		if (user == null)
		{
			return Response<UserAppDto>.Fail("UserName not found", StatusCodes.Status404NotFound, true);
		}

		return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), StatusCodes.Status200OK);
	}
}
