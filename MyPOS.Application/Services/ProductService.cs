using MyPOS.Application.DTOs.Products;
using MyPOS.Application.Interfaces;
using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPOS.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            return new ProductDto // Manual mapping
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            };
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(product => new ProductDto // Manual mapping
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            });
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var product = new Product // Manual mapping
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description
            };
            await _productRepository.AddAsync(product);
            // Assuming AddAsync updates the product with an Id
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            };
        }

        public async Task UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                // Handle not found, e.g., throw an exception
                return;
            }
            // Manual mapping
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Description = productDto.Description;

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                await _productRepository.DeleteAsync(product);
            }
        }
    }
}
