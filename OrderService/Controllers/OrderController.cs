using OrderServer.API.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Controllers;
using OrderServer.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace OrderServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : CustomBaseController
{
	private readonly IOrderService _orderService;


	public OrderController(IOrderService orderService)
	{
		_orderService = orderService;
	}


	[HttpGet("GetOrder")]
	public async Task<IActionResult> GetOrder() => ActionResultInstance(await _orderService.GetOrders());


	#region User

	[Authorize(Roles = "User")]
	[HttpPost("CreateOrder")]
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

	#endregion


	#region Admin

	[Authorize(Roles = "Admin")]
	[HttpPut("UpdateOrderStatus")]
	public async Task<IActionResult> UpdateOrderStatus(UpdateStatusDto dto) => ActionResultInstance(await _orderService.ChangeStatusOrder(dto));

	[Authorize(Roles = "Admin")]
	[HttpGet("GetOrderForAdmin")]
	public async Task<IActionResult> GetOrderForAdmin() => ActionResultInstance(await _orderService.GetOrderAsyncForAdmin());


	[Authorize(Roles = "Admin")]
	[HttpGet("GetCourierWithOrderStatus/{courierId}")]
	public IActionResult GetCourierWithOrderStatus(string courierId) => ActionResultInstance(_orderService.GetCourierWithOrderStatus(courierId));



	[Authorize(Roles = "Admin")]
	[HttpPost("SendTheOrderToTheCourier")]
	public async Task<IActionResult> SendTheOrderToTheCourier(SendTheOrderToTheCourierDto dto) => ActionResultInstance(await _orderService.SendTheOrderToTheCourier(dto));


	#endregion


	#region Courier

	[Authorize(Roles = "Courier")]
	[HttpGet("ShowOrderDetail")]
	public async Task<IActionResult> ShowOrderDetail()
	{
		//var courierName = HttpContext.User.Identity!.Name;
		var courierId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!;

		return ActionResultInstance(await _orderService.ShowOrderDetailAsync(courierId.Value));
	}

	#endregion


	#region UserAndCourier

	[Authorize(Roles = "User,Courier")]
	[HttpGet("ShowDetailDelivery/{orderId}")]
	public async Task<IActionResult> ShowDetailDelivery(string orderId)
	{
		var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

		return ActionResultInstance(await _orderService.ShowDetailDelivery(userId!.Value, orderId));
	}

	#endregion

}
