using MyPOS.Domain.Models;

namespace MyPOS.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        // Add any product-specific methods here
    }
}
