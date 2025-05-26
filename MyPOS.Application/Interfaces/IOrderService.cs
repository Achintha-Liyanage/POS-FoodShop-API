using MyPOS.Application.DTOs.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPOS.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto);
        // Update and Delete for Orders might be complex and depend on business rules (e.g., can't update a processed order).
        // For now, we'll keep it simple or omit them if not strictly required by current scope.
        // Task UpdateOrderAsync(int id, UpdateOrderDto orderDto); // Assuming UpdateOrderDto exists if needed
        // Task DeleteOrderAsync(int id);
    }
}
