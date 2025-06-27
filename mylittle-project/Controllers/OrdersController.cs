using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET with pagination
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paginated = await _orderService.GetPaginatedOrdersAsync(page, pageSize);
            return Ok(paginated);
        }

        // GET single order by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            var newOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
        }

        // PUT update order
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, Order order)
        {
            if (id != order.Id) return BadRequest();
            var updated = await _orderService.UpdateOrderAsync(order);
            return updated ? NoContent() : NotFound();
        }

        // DELETE order
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
