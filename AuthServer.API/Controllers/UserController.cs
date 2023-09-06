using AuthServer.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using AuthServer.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : CustomBaseController
{
	private readonly IUserService _userService;

	public UserController(IUserService userService)
	{
		_userService = userService;
	}

	[HttpPost]
	public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
	{
		return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> GetUser()
	{
		return ActionResultInstance(await _userService.GetUserByNameAsync(HttpContext.User.Identity!.Name!));
	}

	
	[Authorize(Roles = "Admin")]
	[HttpPost("CreateUserRoles")]
	public async Task<IActionResult> CreateUserRoles(CreateUserRoleDto dto)
	{
		return ActionResultInstance(await _userService.CreateUserRoles(dto));
	}
}
