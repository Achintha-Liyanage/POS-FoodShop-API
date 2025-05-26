using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;

namespace MyPOS.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement any product-specific methods here
    }
}
