using SharedLibrary.Models.Enum;

namespace SharedLibrary.Models;

public class OutBox
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;

	// Userin Oldugu address
	public string DestinationAddress { get; set; } = null!;
	public decimal TotalAmount { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime? DeliveryDate { get; set; }
	public OrderStatus Status { get; set; }



	public string UserId { get; set; } = null!;
	public string UserName { get; set; } = null!;

	public string? CourierId { get; set; }
	public string? CourierName { get; set; }
	public bool IsSend { get; set; } = false;
	public bool IsDelete { get; set; } = false;
}
