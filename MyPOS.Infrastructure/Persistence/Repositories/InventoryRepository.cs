using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;

namespace MyPOS.Infrastructure.Persistence.Repositories
{
    public class InventoryRepository : Repository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement any inventory-specific methods here
    }
}
