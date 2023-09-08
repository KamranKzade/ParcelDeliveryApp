using OrderService.API.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : CustomBaseController
{
	private readonly IOrderService _orderService;

	public OrderController(IOrderService orderService)
	{
		_orderService = orderService;
	}

	// [Authorize(Roles = "Admin")]
	// [HttpGet]
	// public IActionResult GetStock()
	// {
	// 	var userName = HttpContext.User.Identity.Name;
	// 	var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
	// 	var birthDate = User.Claims.FirstOrDefault(x => x.Type == "birth-date");
	// 

	// Userin Role-larini almaq
	// var roles = new List<string>();
	// List<Claim> roleClaims = HttpContext.User.FindAll(ClaimTypes.Role).ToList();
	// roleClaims.ForEach(x=> roles.Add(x.Value));

	// 	// db-dan userId veya username uzerinden lazimli datalari cek
	// 	return Ok($"Stock islemleri ==> Username: {userName} -- UserId:{userId.Value} -- BirthDate:{birthDate}");
	// }

	[Authorize(Roles = "User")]
	[HttpPost]
	public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
	{
		var userName = HttpContext.User.Identity.Name;
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
		var address = User.Claims.FirstOrDefault(x => x.Type == "Address");

		return ActionResultInstance(await _orderService.CreateOrderAsync(dto, userName, userId.Value, address.Value));
	}

	[Authorize(Roles = "User")]
	[HttpGet("GetOrderForUser")]
	public async Task<IActionResult> GetOrderForUser()
	{
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

		return ActionResultInstance(await _orderService.GetOrderAsyncForUser(userId.Value));
	}


	[Authorize(Roles = "User")]
	[HttpPut("UpdateAddress")]
	// RabbitMq ile elaqe yaratmaq qalibdi
	public async Task<IActionResult> UpdateAddress(UpdateOrderDto dto)
	{
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
		var addressOld = User.Claims.FirstOrDefault(x => x.Type == "Address");


		return ActionResultInstance(await _orderService.UpdateAddressAsync(userId!.Value, dto.OrderId, dto.Address));
	}

	[Authorize(Roles = "User")]
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteOrder(string id)
	{
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

		return ActionResultInstance(await _orderService.DeleteOrderAsync(userId!.Value, id));
	}


	[Authorize(Roles = "Admin")]
	[HttpPut("UpdateOrderStatus")]
	public async Task<IActionResult> UpdateOrderStatus(UpdateStatusDto dto)
	{
		return ActionResultInstance(await _orderService.ChangeStatusOrder(dto));
	}

}
