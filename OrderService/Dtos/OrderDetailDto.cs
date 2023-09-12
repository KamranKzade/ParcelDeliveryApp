using SharedLibrary.Models.Enum;

namespace OrderService.API.Dtos;

public class OrderDetailDto
{
    public string CourierName { get; set; }
    public string OrderName { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public string DestinationAddress { get; set; }
    public decimal TotalAmount { get; set; }

}
