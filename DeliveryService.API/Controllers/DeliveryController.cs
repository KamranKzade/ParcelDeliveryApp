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
	public DeliveryController(IDeliveryService service)
	{
		_service = service;
	}

	public IDeliveryService _service { get; set; }



	[Authorize(Roles = "Admin")]
	[HttpGet]
	public IActionResult GetStock()
	{
		var userName = HttpContext.User.Identity.Name;
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
		var birthDate = User.Claims.FirstOrDefault(x => x.Type == "birth-date");


		// db-dan userId veya username uzerinden lazimli datalari cek
		return Ok($"Stock islemleri ==> Username: {userName} -- UserId:{userId.Value} -- BirthDate:{birthDate}");
	}


	[Authorize(Roles = "Courier")]
	[HttpPost]
	public async Task<IActionResult> ChangeOrderStatus(ChangeOrderStatusDto dto)
	{
		var courierId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

		return ActionResultInstance(await _service.ChangeOrderStatus(dto, courierId!.Value));
	}

}
