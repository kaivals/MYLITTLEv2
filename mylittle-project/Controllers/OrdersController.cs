
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

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

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            var newOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, Order order)
        {
            if (id != order.Id) return BadRequest();
            var updated = await _orderService.UpdateOrderAsync(order);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }

}
