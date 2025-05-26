using MyPOS.Application.DTOs.Inventories;
using MyPOS.Application.Interfaces;
using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for IEnumerable

namespace MyPOS.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository; // To get product names

        public InventoryService(IInventoryRepository inventoryRepository, IProductRepository productRepository)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
        }

        public async Task<InventoryDto> GetInventoryByProductIdAsync(int productId)
        {
            // Assuming one-to-one or one-to-many where we take the first if multiple (should be one-to-one)
            var inventoryItems = await _inventoryRepository.GetAllAsync();
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null) return null;

            var product = await _productRepository.GetByIdAsync(productId);

            return new InventoryDto // Manual mapping
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = product?.Name ?? "N/A",
                QuantityInStock = inventory.QuantityInStock,
                LastStockUpdatedAt = inventory.LastStockUpdatedAt
            };
        }
        
        // Added method to get all inventory items, as it's needed for InventoryController GET /api/inventory
        public async Task<IEnumerable<InventoryDto>> GetAllInventoryAsync()
        {
            var inventoryItems = await _inventoryRepository.GetAllAsync();
            var productIds = inventoryItems.Select(i => i.ProductId).Distinct();
            var products = new List<Product>();
            foreach(var pId in productIds)
            {
                 var p = await _productRepository.GetByIdAsync(pId);
                 if(p != null) products.Add(p);
            }


            return inventoryItems.Select(inventory =>
            {
                var product = products.FirstOrDefault(p => p.Id == inventory.ProductId);
                return new InventoryDto
                {
                    Id = inventory.Id,
                    ProductId = inventory.ProductId,
                    ProductName = product?.Name ?? "N/A",
                    QuantityInStock = inventory.QuantityInStock,
                    LastStockUpdatedAt = inventory.LastStockUpdatedAt
                };
            });
        }


        public async Task UpdateInventoryAsync(int productId, UpdateInventoryDto inventoryDto)
        {
            var inventoryItems = await _inventoryRepository.GetAllAsync();
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
            {
                // Or create a new inventory record if business logic allows
                // For now, assume inventory record must exist to be updated.
                throw new ApplicationException($"Inventory record for ProductId {productId} not found.");
            }

            // Manual mapping
            inventory.QuantityInStock = inventoryDto.QuantityInStock;
            inventory.LastStockUpdatedAt = DateTime.UtcNow;

            await _inventoryRepository.UpdateAsync(inventory);
        }
    }
}
