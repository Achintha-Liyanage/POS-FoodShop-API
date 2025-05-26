using System.Collections.Generic;

namespace MyPOS.Application.DTOs.Orders
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; }
    }
}
