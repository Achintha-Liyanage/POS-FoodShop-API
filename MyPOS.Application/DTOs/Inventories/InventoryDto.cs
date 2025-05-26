using System;

namespace MyPOS.Application.DTOs.Inventories
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Assuming we'll populate this
        public int QuantityInStock { get; set; }
        public DateTime LastStockUpdatedAt { get; set; }
    }
}
