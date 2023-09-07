using OrderService.Models.Enum;

namespace OrderService.Dtos;

public class OrderDto
{

	public int Id { get; set; }
	public string Name { get; set; }

	// Userin Oldugu address
	public string DestinationAddress { get; set; }
	public decimal TotalAmount { get; set; }
	public DateTime CreatedDate { get; set; }
	public OrderStatus Status { get; set; }



	public string UserId { get; set; }
	public string UserName { get; set; }

	public string? CourierId { get; set; } 
	public string? CourierName { get; set; } 

}
