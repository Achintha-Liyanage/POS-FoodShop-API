using Microsoft.AspNetCore.Mvc;
using MyPOS.Application.DTOs.Orders;
using MyPOS.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (System.ApplicationException ex) // Catch specific exceptions thrown by the service
            {
                // For example, if customer or product not found, or insufficient stock
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Generic error for unexpected issues
                // Log the exception ex
                return StatusCode(500, "An unexpected error occurred while creating the order.");
            }
        }
    }
}
