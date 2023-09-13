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
	public readonly IDeliveryService _service ;

	public DeliveryController(IDeliveryService service)
	{
		_service = service;
	}

	[Authorize(Roles = "Courier")]
	[HttpPost]
	public async Task<IActionResult> ChangeOrderStatus(ChangeOrderStatusDto dto)
	{
		var courierId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

		return ActionResultInstance(await _service.ChangeOrderStatus(dto, courierId!.Value));
	}

}
