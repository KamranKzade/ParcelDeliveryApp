using SharedLibrary.Models.Enum;

namespace DeliveryServer.API.Dtos;

public class ChangeOrderStatusDto
{
    public string? OrderId { get; set; }
    public OrderStatus OrderStatus { get; set; }
}
