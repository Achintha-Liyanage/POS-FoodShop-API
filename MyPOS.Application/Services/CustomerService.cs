using MyPOS.Application.DTOs.Customers;
using MyPOS.Application.Interfaces;
using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPOS.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return null;
            return new CustomerDto // Manual mapping
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(customer => new CustomerDto // Manual mapping
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            });
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            var customer = new Customer // Manual mapping
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                PhoneNumber = customerDto.PhoneNumber
            };
            await _customerRepository.AddAsync(customer);
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }

        public async Task UpdateCustomerAsync(int id, UpdateCustomerDto customerDto)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                // Handle not found
                return;
            }
            // Manual mapping
            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.PhoneNumber = customerDto.PhoneNumber;

            await _customerRepository.UpdateAsync(customer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer != null)
            {
                await _customerRepository.DeleteAsync(customer);
            }
        }
    }
}
