using SharedLibrary.Models.Enum;

namespace OrderService.API.Models;

public class Order
{
	public Guid Id { get; set; }
	public string Name { get; set; }

	// Userin Oldugu address
	public string DestinationAddress { get; set; }
	public decimal TotalAmount { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime? DeliveryDate { get; set; }
	public OrderStatus Status { get; set; }



	public string UserId { get; set; }
	public string UserName { get; set; }

	public string? CourierId { get; set; }
	public string? CourierName { get; set; }

}
