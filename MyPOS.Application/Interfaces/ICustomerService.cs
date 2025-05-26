using MyPOS.Application.DTOs.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPOS.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task UpdateCustomerAsync(int id, UpdateCustomerDto customerDto);
        Task DeleteCustomerAsync(int id);
    }
}
