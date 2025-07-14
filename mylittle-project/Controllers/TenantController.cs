using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
   
    [ApiController]
    [Route("api/tenants")]
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
        [Authorize(Roles = "TenantOwner")]
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
        [Authorize(Roles = "TenantOwner")]
        [HttpGet("{tenantId:guid}", Name = "GetTenantById")]
        public async Task<ActionResult<TenantDto>> GetTenantById(Guid tenantId)
        {
            var tenant = await _tenantService.GetTenantWithFeaturesAsync(tenantId);
            return tenant == null ? NotFound() : Ok(tenant);
        }

        // ───────────────────────────────────────────────────────────────
        // POST /api/v1/tenants
        // ───────────────────────────────────────────────────────────────
        [Authorize(Roles = "TenantOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTenant([FromBody] TenantDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var tenant = await _tenantService.CreateAsync(dto, Guid.Parse(userId));
            return Ok(tenant);
        }


        // ───────────────────────────────────────────────────────────────
        // PUT /api/v1/tenants/{tenantId}
        // ───────────────────────────────────────────────────────────────
        [Authorize(Roles = "StoreManager , TenantOwner")]
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
