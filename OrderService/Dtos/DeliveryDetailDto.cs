using SharedLibrary.Models.Enum;

namespace OrderService.API.Dtos;

public class DeliveryDetailDto
{
	public string OrderName { get; set; }
	public string CourierName { get; set; }

	public OrderStatus OrderStatus { get; set; }
	public string DestinationAddress { get; set; }
	public DateTime CreateDate { get; set; }
	public DateTime? DeliveryDate { get; set; }
}
