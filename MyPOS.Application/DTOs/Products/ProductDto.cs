namespace MyPOS.Application.DTOs.Products
{
    /// <summary>
    /// Represents a product in the system.
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <example>Super Widget</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        /// <example>19.99</example>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the optional description for the product.
        /// </summary>
        /// <example>A high-quality widget with many features.</example>
        public string? Description { get; set; }
    }
}
