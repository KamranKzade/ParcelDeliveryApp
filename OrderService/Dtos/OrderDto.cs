using SharedLibrary.Models.Enum;

namespace OrderServer.API.Dtos;

public class OrderDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;

	// Userin Oldugu address
	public string? DestinationAddress { get; set; } 
	public decimal TotalAmount { get; set; }
	public DateTime CreatedDate { get; set; }
	public OrderStatus Status { get; set; }



	public string UserId { get; set; } = null!;
	public string UserName { get; set; } = null!;

	public string? CourierId { get; set; }
	public string? CourierName { get; set; }
}
