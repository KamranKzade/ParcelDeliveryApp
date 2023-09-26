namespace OrderServer.API.Dtos;

public class CreateOrderDto
{
	public string Name { get; set; } = null!;

	// Userin Oldugu address
	public decimal TotalAmount { get; set; }
}
