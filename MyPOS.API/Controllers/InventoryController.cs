using Microsoft.AspNetCore.Mvc;
using MyPOS.Application.DTOs.Inventories;
using MyPOS.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventory()
        {
            var inventoryItems = await _inventoryService.GetAllInventoryAsync();
            return Ok(inventoryItems);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<InventoryDto>> GetInventoryByProductId(int productId)
        {
            var inventoryItem = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            return Ok(inventoryItem);
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult<InventoryDto>> UpdateInventory(int productId, [FromBody] UpdateInventoryDto updateInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Ensure the inventory for the product exists before attempting update.
                var existingInventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
                if (existingInventory == null)
                {
                     // Depending on business logic, you might create it or return NotFound.
                     // Let's assume for now it must exist.
                    return NotFound($"Inventory for Product ID {productId} not found.");
                }

                await _inventoryService.UpdateInventoryAsync(productId, updateInventoryDto);
                
                // Fetch the updated inventory item to return it
                var updatedInventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
                if (updatedInventory == null)
                {
                    // This case should ideally not happen if Update was successful and item existed.
                    return NotFound($"Failed to retrieve updated inventory for Product ID {productId}.");
                }
                return Ok(updatedInventory);
            }
            catch (System.ApplicationException ex) // Catch specific exceptions from service layer
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Log the exception ex
                return StatusCode(500, "An unexpected error occurred while updating the inventory.");
            }
        }
    }
}
