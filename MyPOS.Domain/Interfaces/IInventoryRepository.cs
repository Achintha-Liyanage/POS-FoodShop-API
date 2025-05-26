using MyPOS.Domain.Models;

namespace MyPOS.Domain.Interfaces
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        // Add any inventory-specific methods here
    }
}
