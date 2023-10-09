using AuthServer.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Controllers;
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

	[HttpPost("CreateUser")]
	public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
	{
		return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
	}


	[Authorize(Roles = "Admin")]
	[HttpPost("CreateCourier")]
	public async Task<IActionResult> CreateCourier(CreateUserDto createUserDto)
	{
		return ActionResultInstance(await _userService.CreateCourierAsync(createUserDto));
	}



	[HttpGet()]
	[Authorize]
	public async Task<IActionResult> GetUser()
	{
		return ActionResultInstance(await _userService.GetUserByNameAsync(HttpContext.User.Identity!.Name!));
	}


	[Authorize(Roles = "Admin")]
	[HttpGet("GetCourierList")]
	public async Task<IActionResult> GetCourierList()
	{
		return ActionResultInstance(await _userService.GetCourierList());
	}

	[Authorize(Roles = "Admin")]
	[HttpPost("CreateUserRoles")]
	public async Task<IActionResult> CreateUserRoles(CreateUserRoleDto dto)
	{
		return ActionResultInstance(await _userService.CreateUserRoles(dto));
	}
}
