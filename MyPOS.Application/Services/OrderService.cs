using MyPOS.Application.DTOs.Orders;
using MyPOS.Application.Interfaces;
using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPOS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICustomerRepository _customerRepository; // To check if customer exists

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _customerRepository = customerRepository;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            // Fetch order items separately if not included by default by your GetByIdAsync
            // For this example, let's assume Order.OrderItems is populated or we fetch them.
            // This might require an OrderRepository method like GetOrderWithItemsAsync(id).
            // For now, let's assume they are loaded or handle it manually if OrderItems is null.

            var orderItems = await _orderItemRepository.GetAllAsync(); // Inefficient: Get all and filter
            var itemsForThisOrder = orderItems.Where(oi => oi.OrderId == id).ToList();


            return new OrderDto // Manual mapping
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                OrderItems = itemsForThisOrder.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "N/A", // Product might not be loaded
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var allOrderItems = await _orderItemRepository.GetAllAsync(); // Inefficient
            var allProducts = await _productRepository.GetAllAsync(); // Inefficient

            return orders.Select(order => new OrderDto // Manual mapping
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                OrderItems = allOrderItems.Where(oi => oi.OrderId == order.Id)
                                          .Select(oi => {
                                              var product = allProducts.FirstOrDefault(p => p.Id == oi.ProductId);
                                              return new OrderItemDto {
                                                Id = oi.Id,
                                                ProductId = oi.ProductId,
                                                ProductName = product?.Name ?? "N/A",
                                                Quantity = oi.Quantity,
                                                UnitPrice = oi.UnitPrice,
                                                TotalPrice = oi.Quantity * oi.UnitPrice
                                              };
                                          }).ToList()
            });
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            // 1. Validate Customer
            var customer = await _customerRepository.GetByIdAsync(orderDto.CustomerId);
            if (customer == null)
            {
                throw new ApplicationException("Customer not found.");
            }

            var orderItems = new List<OrderItem>();
            decimal totalOrderAmount = 0;

            // 2. Validate Products and Stock, Calculate Total
            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new ApplicationException($"Product with Id {itemDto.ProductId} not found.");
                }

                var inventory = (await _inventoryRepository.GetAllAsync()).FirstOrDefault(inv => inv.ProductId == itemDto.ProductId);
                if (inventory == null || inventory.QuantityInStock < itemDto.Quantity)
                {
                    throw new ApplicationException($"Insufficient stock for product {product.Name}. Available: {inventory?.QuantityInStock ?? 0}, Requested: {itemDto.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price // Assuming price from Product is the unit price
                };
                orderItems.Add(orderItem);
                totalOrderAmount += orderItem.Quantity * orderItem.UnitPrice;
            }

            // 3. Create Order
            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalOrderAmount,
                OrderItems = new List<OrderItem>() // Initialize to empty list, will be added below
            };

            await _orderRepository.AddAsync(order); // This should assign an Id to 'order'

            // 4. Create OrderItems and associate with the Order
            foreach (var oi in orderItems)
            {
                oi.OrderId = order.Id; // Link OrderItem to the new Order
                await _orderItemRepository.AddAsync(oi);
                order.OrderItems.Add(oi); // Add to the order's collection for the DTO response

                // 5. Update Inventory
                var inventoryItem = (await _inventoryRepository.GetAllAsync()).First(inv => inv.ProductId == oi.ProductId);
                inventoryItem.QuantityInStock -= oi.Quantity;
                inventoryItem.LastStockUpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventoryItem);
            }
            
            // It's good practice to save changes for the order again if OrderItems were added/modified after initial AddAsync.
            // However, our current IRepository.AddAsync calls SaveChangesAsync.
            // If OrderItems are part of the Order aggregate and EF Core is configured for cascading saves,
            // adding OrderItems to order.OrderItems and then calling _orderRepository.UpdateAsync(order) might be an alternative.
            // For now, adding OrderItems separately and updating inventory is explicit.

            return new OrderDto // Manual mapping for response
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    // Product name might require another fetch or smarter loading
                    ProductName = ( _productRepository.GetByIdAsync(oi.ProductId)).Result?.Name ?? "N/A",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };
        }
    }
}
