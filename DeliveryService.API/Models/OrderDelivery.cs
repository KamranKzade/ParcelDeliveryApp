using SharedLibrary.Models.Enum;

namespace DeliveryServer.API.Models;

public class OrderDelivery
{
	public Guid Id { get; set; }
	public string Name { get; set; }

	// Userin Oldugu address
	public DateTime? DeliveryDate { get; set; }
	public OrderStatus Status { get; set; }

	public string? CourierId { get; set; }
	public string? CourierName { get; set; }
}
