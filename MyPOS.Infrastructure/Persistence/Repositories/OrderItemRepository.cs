using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;

namespace MyPOS.Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement any order item-specific methods here
    }
}
