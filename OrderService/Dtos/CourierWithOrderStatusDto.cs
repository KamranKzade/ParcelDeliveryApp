using SharedLibrary.Models.Enum;

namespace OrderServer.API.Dtos;

public class CourierWithOrderStatusDto
{
    public string? CourierId { get; set; }
    public string? CourierName { get; set; }
	public string? OrderName { get; set; }
	public OrderStatus OrderStatus { get; set; }

}
