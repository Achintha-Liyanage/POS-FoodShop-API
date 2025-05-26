using Microsoft.AspNetCore.Mvc;
using MyPOS.Application.DTOs.Products;
using MyPOS.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Added for authorization
using Microsoft.Extensions.Logging; // Added for ILogger

namespace MyPOS.API.Controllers
{
    /// <summary>
    /// Manages products in the MyPOS system.
    /// Allows creating, retrieving, updating, and deleting products.
    /// All operations require authentication.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Secure the whole controller
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger; // Added ILogger

        public ProductsController(IProductService productService, ILogger<ProductsController> logger) // Injected ILogger
        {
            _productService = productService;
            _logger = logger; // Assigned ILogger
        }

        /// <summary>
        /// Retrieves a list of all products.
        /// </summary>
        /// <returns>A list of products.</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            _logger.LogInformation("Attempting to fetch all products."); // Added log statement
            var products = await _productService.GetAllProductsAsync();
            _logger.LogInformation($"Successfully fetched {products.Count()} products.");
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            _logger.LogInformation($"Attempting to fetch product with ID: {id}.");
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID: {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Successfully fetched product with ID: {id}.");
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            _logger.LogInformation("Attempting to create a new product.");
            // Example of role-based authorization for a specific endpoint
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning($"User {User.Identity?.Name} without Admin role attempted to create a product.");
                return Forbid(); // Or Unauthorized if you prefer a 401
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateProduct called with invalid model state.");
                return BadRequest(ModelState);
            }
            var createdProduct = await _productService.CreateProductAsync(createProductDto);
            _logger.LogInformation($"Successfully created product with ID: {createdProduct.Id}.");
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            _logger.LogInformation($"Attempting to update product with ID: {id}.");
            if (!ModelState.IsValid)
            {
                 _logger.LogWarning($"UpdateProduct called with invalid model state for ID: {id}.");
                return BadRequest(ModelState);
            }
            try
            {
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning($"Product with ID: {id} not found for update.");
                    return NotFound($"Product with ID {id} not found.");
                }
                await _productService.UpdateProductAsync(id, updateProductDto);
                _logger.LogInformation($"Successfully updated product with ID: {id}.");
            }
            catch (System.ApplicationException ex) 
            {
                _logger.LogError(ex, $"Application error while updating product ID: {id}.");
                return NotFound(ex.Message); 
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"Attempting to delete product with ID: {id}.");
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID: {id} not found for deletion.");
                return NotFound();
            }
            await _productService.DeleteProductAsync(id);
            _logger.LogInformation($"Successfully deleted product with ID: {id}.");
            return NoContent();
        }
    }
}
