using OrderService.API.Models.Enum;

namespace OrderService.API.Dtos;

public class CreateOrderDto
{
	public string Name { get; set; }

	// Userin Oldugu address
	public decimal TotalAmount { get; set; }
}
