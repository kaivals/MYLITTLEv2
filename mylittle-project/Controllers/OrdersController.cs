using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.DTOs.Common;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ──────────────── POST ENDPOINTS ────────────────

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetOrderById), new { id }, id);
        }

        [HttpPost("filter")]
        public async Task<ActionResult<PaginatedResult<OrderDto>>> GetPaginatedOrders([FromBody] OrderFilterDto filter)
        {
            var result = await _orderService.GetPaginatedOrdersAsync(filter);
            return Ok(result);
        }

        // ──────────────── GET ENDPOINTS ────────────────

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound($"Order with ID {id} not found.");

            return Ok(order);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // ──────────────── PUT ENDPOINTS ────────────────

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.UpdateOrderAsync(id, dto);
            if (!result)
                return NotFound($"Order with ID {id} not found.");

            return NoContent();
        }

        // ──────────────── PATCH / DELETE ENDPOINTS ────────────────

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
                return NotFound($"Order with ID {id} not found.");

            return NoContent();
        }

        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteOrder(Guid id)
        {
            var result = await _orderService.SoftDeleteOrderAsync(id);
            if (!result)
                return NotFound($"Order with ID {id} not found or already deleted.");

            return Ok(new { message = "Order soft-deleted successfully." });
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreOrder(Guid id)
        {
            var result = await _orderService.RestoreOrderAsync(id);
            if (!result)
                return NotFound($"Order with ID {id} not found or not deleted.");

            return Ok(new { message = "Order restored successfully." });
        }
    }
}
