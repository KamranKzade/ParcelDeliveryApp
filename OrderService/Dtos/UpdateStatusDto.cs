﻿using SharedLibrary.Models.Enum;

namespace OrderServer.API.Dtos;


public class UpdateStatusDto
{
    public string OrderId { get; set; }
    public OrderStatus OrderStatus { get; set; }
}
