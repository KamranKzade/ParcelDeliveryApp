namespace OrderServer.API.Dtos;

public class SendTheOrderToTheCourierDto
{
	public string? OrderId { get; set; }
	public string? CourierId { get; set; }
	public string? CourierName { get; set; }
}
