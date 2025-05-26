namespace MyPOS.Application.DTOs.Products
{
    /// <summary>
    /// Represents the data required to create a new product.
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Gets or sets the name of the product. This field is required.
        /// </summary>
        /// <example>Mega Widget</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price of the product. Must be greater than 0.
        /// </summary>
        /// <example>25.50</example>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets an optional description for the new product.
        /// </summary>
        /// <example>The latest and greatest widget.</example>
        public string? Description { get; set; }
    }
}
