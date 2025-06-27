using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.API.Controllers
{
    // Base URL: /api/v1/tenants/…
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
        // 1)  GET  /api/v1/tenants
        // ───────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllAsync()
        {
            var tenants = await _tenantService.GetAllAsync();
            return Ok(tenants);
        }

        // ───────────────────────────────────────────────────────────────
        // 2)  GET  /api/v1/tenants/{tenantId}
        //     (needed for CreatedAtRoute)
        // ───────────────────────────────────────────────────────────────
        [HttpGet("{tenantId:guid}", Name = "GetTenantById")]
        public async Task<ActionResult<TenantDto>> GetTenantById(Guid tenantId)
        {
            var tenant = await _tenantService.GetTenantWithFeaturesAsync(tenantId);
            return tenant == null ? NotFound() : Ok(tenant);
        }

        // ───────────────────────────────────────────────────────────────
        // 3)  POST /api/v1/tenants
        //     Returns 201 + Location header pointing to GetTenantById
        // ───────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] TenantDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tenant = await _tenantService.CreateAsync(dto);

            // 201 Created + Location: /api/v1/tenants/{tenantId}
            return CreatedAtRoute("GetTenantById",
                                  new { tenantId = tenant.Id },
                                  tenant);
        }
        // ───────────────────────────────────────────────────────────────
        //  Put  /api/v1/tenants/{tenantId}/update
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(Guid tenantId, [FromBody] TenantDto dto)
        {
            var success = await _tenantService.UpdateTenantAsync(tenantId, dto);
            if (!success)
                return NotFound("Tenant not found or update failed.");

            return Ok("Tenant updated successfully.");
        }


        // ───────────────────────────────────────────────────────────────
        // 4)  GET  /api/v1/tenants/{tenantId}/features
        // ───────────────────────────────────────────────────────────────
        [HttpGet("{tenantId:guid}/features")]
        public async Task<ActionResult<List<FeatureModuleDto>>> GetFeatureTreeAsync(Guid tenantId)
        {
            var tree = await _tenantService.GetFeatureTreeAsync(tenantId);
            return tree.Count > 0 ? Ok(tree) : NotFound();
        }

        // ───────────────────────────────────────────────────────────────
        // 5)  PUT  /api/v1/tenants/{tenantId}/modules/{moduleId}
        //      Body: { "isEnabled": true/false }
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId:guid}/modules/{moduleId:guid}")]
        public async Task<ActionResult> ToggleModuleAsync(
            Guid tenantId, Guid moduleId, [FromBody] ToggleDto dto)
        {
            var ok = await _tenantService.UpdateModuleToggleAsync(tenantId, moduleId, dto.IsEnabled);
            return ok ? NoContent() : BadRequest("Toggle failed (module not found or parent rule).");
        }

        // ───────────────────────────────────────────────────────────────
        // 6)  PUT  /api/v1/tenants/{tenantId}/features/{featureId}
        //      Body: { "isEnabled": true/false }
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId:guid}/features/{featureId:guid}")]
        public async Task<ActionResult> ToggleFeatureAsync(
            Guid tenantId, Guid featureId, [FromBody] ToggleDto dto)
        {
            var ok = await _tenantService.UpdateFeatureToggleAsync(tenantId, featureId, dto.IsEnabled);
            return ok ? NoContent() : BadRequest("Toggle failed (feature not found or parent OFF).");
        }

        // ───────────────────────────────────────────────────────────────
        // 7)  PUT  /api/v1/tenants/{tenantId}/store
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId:guid}/store")]
        public async Task<ActionResult> UpdateStoreAsync(Guid tenantId, [FromBody] StoreDto dto)
        {
            var ok = await _tenantService.UpdateStoreAsync(tenantId, dto);
            return ok ? NoContent() : NotFound();
        }

        // ───────────────────────────────────────────────────────────────
        // Helper DTO used by the two toggle endpoints
        // ───────────────────────────────────────────────────────────────
        public record ToggleDto
        {
            public bool IsEnabled { get; init; }
        }
    }
}
