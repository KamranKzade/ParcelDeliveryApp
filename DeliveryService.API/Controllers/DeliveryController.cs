﻿using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryController : CustomBaseController
{
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
}
