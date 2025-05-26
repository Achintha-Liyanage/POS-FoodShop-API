using MyPOS.Domain.Models;

namespace MyPOS.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Add any order-specific methods here
    }
}
