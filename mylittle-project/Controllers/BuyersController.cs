using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/buyers")]
    public class BuyersController : ControllerBase
    {
        private readonly IBuyerService _buyerService;

        public BuyersController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

        // ──────────────── POST ENDPOINTS ────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BuyerCreateDto dto)
        {
            var id = await _buyerService.CreateBuyerAsync(dto);
            return Ok(new { BuyerId = id });
        }

        // ──────────────── GET ENDPOINTS ────────────────

        [HttpGet("{buyerId}")]
        public async Task<IActionResult> GetProfile(Guid buyerId)
        {
            var profile = await _buyerService.GetBuyerProfileAsync(buyerId);
            if (profile == null)
                return NotFound(new { Message = "Buyer not found." });

            return Ok(profile);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var buyers = await _buyerService.GetAllBuyersPaginatedAsync(page, pageSize);
            return Ok(buyers);
        }

        [HttpGet("all-portals")]
        public async Task<IActionResult> GetAllBuyers([FromQuery] bool includePortal = false)
        {
            var buyers = await _buyerService.GetAllBuyersAsync(includePortal);
            return Ok(buyers);
        }

        [HttpGet("by-tenant-manager")]
        public async Task<IActionResult> GetBuyersForTenantManager([FromQuery] Guid tenantId, [FromQuery] Guid? dealerId = null)
        {
            var buyers = await _buyerService.GetBuyersForTenantManagerAsync(tenantId, dealerId);
            return Ok(buyers);
        }

        // ──────────────── PUT ENDPOINTS ────────────────

        [HttpPut("{buyerId}")]
        public async Task<IActionResult> Update(Guid buyerId, [FromBody] BuyerUpdateDto dto)
        {
            await _buyerService.UpdateBuyerAsync(buyerId, dto);
            return Ok(new { Message = "Buyer updated successfully." });
        }

        // ──────────────── DELETE ENDPOINTS ────────────────

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _buyerService.SoftDeleteBuyerAsync(id);
            if (!result)
                return NotFound(new { Message = "Buyer not found or already deleted." });

            return Ok(new { Message = "Buyer soft-deleted successfully." });
        }
    }
}
