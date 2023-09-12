using SharedLibrary.Models.Enum;

namespace OrderService.API.Dtos;


public class UpdateStatusDto
{
    public string OrderId { get; set; }
    public OrderStatus OrderStatus { get; set; }
}
