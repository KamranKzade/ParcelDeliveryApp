using System.Security.Claims;
using DeliveryServer.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Controllers;
using Microsoft.AspNetCore.Authorization;
using DeliveryServer.API.Services.Abstract;

namespace DeliveryServer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryController : CustomBaseController
{
	public readonly IDeliveryService _service;

	public DeliveryController(IDeliveryService service)
	{
		_service = service;
	}

	[Authorize(Roles = "Courier")]
	[HttpPost("ChangeOrderStatus")]
	public async Task<IActionResult> ChangeOrderStatus(ChangeOrderStatusDto dto)
	{
		var courierId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
		string authorizationToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

		return ActionResultInstance(await _service.ChangeOrderStatus(dto, courierId!.Value, authorizationToken));
	}

	[Authorize(Roles = "User,Admin")]
	[HttpGet("GetDeliveryOrder")]
	public async Task<IActionResult> GetDeliveryOrder()
	{
		return ActionResultInstance(await _service.GetDeliveryOrder());
	}
}
