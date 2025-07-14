using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/v1/tenants")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        // ───────────────────────────────────────────────────────────────
        // GET /api/v1/tenants (Paginated)
        // ───────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TenantDto>>> GetAllAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _tenantService.GetPaginatedAsync(page, pageSize);
            return Ok(result);
        }

        // ───────────────────────────────────────────────────────────────
        // GET /api/v1/tenants/{tenantId}
        // ───────────────────────────────────────────────────────────────
        [HttpGet("{tenantId:guid}", Name = "GetTenantById")]
        public async Task<ActionResult<TenantDto>> GetTenantById(Guid tenantId)
        {
            var tenant = await _tenantService.GetTenantWithFeaturesAsync(tenantId);
            return tenant == null ? NotFound() : Ok(tenant);
        }

        // ───────────────────────────────────────────────────────────────
        // POST /api/v1/tenants
        // ───────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] TenantDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tenant = await _tenantService.CreateAsync(dto);
            return CreatedAtRoute("GetTenantById", new { tenantId = tenant.Id }, tenant);
        }

        // ───────────────────────────────────────────────────────────────
        // PUT /api/v1/tenants/{tenantId}
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(Guid tenantId, [FromBody] TenantDto dto)
        {
            var success = await _tenantService.UpdateTenantAsync(tenantId, dto);
            if (!success)
                return NotFound("Tenant not found or update failed.");

            return Ok("Tenant updated successfully.");
        }
    }
}
