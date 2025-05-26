using MyPOS.Domain.Models;

namespace MyPOS.Domain.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        // Add any customer-specific methods here
    }
}
