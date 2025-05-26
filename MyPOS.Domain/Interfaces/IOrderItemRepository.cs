using MyPOS.Domain.Models;

namespace MyPOS.Domain.Interfaces
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        // Add any order item-specific methods here
    }
}
