using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;

namespace MyPOS.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement any order-specific methods here
    }
}
