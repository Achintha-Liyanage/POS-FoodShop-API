using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;

namespace MyPOS.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement any customer-specific methods here
    }
}
