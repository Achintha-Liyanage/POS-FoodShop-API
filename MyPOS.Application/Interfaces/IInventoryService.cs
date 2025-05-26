using MyPOS.Application.DTOs.Inventories;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for IEnumerable

namespace MyPOS.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryDto> GetInventoryByProductIdAsync(int productId);
        Task<IEnumerable<InventoryDto>> GetAllInventoryAsync(); // Added
        Task UpdateInventoryAsync(int productId, UpdateInventoryDto inventoryDto);
    }
}
