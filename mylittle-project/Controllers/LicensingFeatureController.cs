using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.API.Controllers
{
    /// <summary>
    /// Endpoints used by the “Portal Licensing & Feature Access” screen.
    /// </summary>
    [ApiController]
    [Route("api/tenant-feature-settings")]
    public class LicensingFeatureController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public LicensingFeatureController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        // ───────────────────────────────────────────────────────────────
        // 1) LEFT COLUMN: List all portals with enabled-module counts
        //    GET  /api/tenant-feature-settings/portals
        // ───────────────────────────────────────────────────────────────
        [HttpGet("portals")]
        public async Task<ActionResult<List<PortalSummaryDto>>> GetPortals()
        {
            var list = await _tenantService.GetPortalSummariesAsync();
            return Ok(list);   // [{ tenantId, tenantName, modulesOn, modulesAll, ... }]
        }

        // ───────────────────────────────────────────────────────────────
        // 2) RIGHT PANEL: Module → feature tree for a selected portal
        //    GET  /api/tenant-feature-settings/{tenantId}/features
        // ───────────────────────────────────────────────────────────────
        [HttpGet("{tenantId:guid}/features")]
        public async Task<ActionResult<List<FeatureModuleDto>>> GetFeatureTree(Guid tenantId)
        {
            var tree = await _tenantService.GetFeatureTreeAsync(tenantId);
            return tree.Count == 0 ? NotFound() : Ok(tree);
        }

        // ───────────────────────────────────────────────────────────────
        // 3) Master toggle (entire module)
        //    PUT  /api/tenant-feature-settings/{tenantId}/modules/{moduleId}
        //    Body: { "isEnabled": true/false }
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId:guid}/modules/{moduleId:guid}")]
        public async Task<IActionResult> ToggleModule(
            Guid tenantId,
            Guid moduleId,
            [FromBody] ToggleDto body)
        {
            var ok = await _tenantService.UpdateModuleToggleAsync(
                         tenantId, moduleId, body.IsEnabled);
            return ok ? NoContent() : BadRequest("Module not found or rule violated.");
        }

        // ───────────────────────────────────────────────────────────────
        // 4) Child toggle (single feature)
        //    PUT  /api/tenant-feature-settings/{tenantId}/features/{featureId}
        //    Body: { "isEnabled": true/false }
        // ───────────────────────────────────────────────────────────────
        [HttpPut("{tenantId:guid}/features/{featureId:guid}")]
        public async Task<IActionResult> ToggleFeature(
            Guid tenantId,
            Guid featureId,
            [FromBody] ToggleDto body)
        {
            var ok = await _tenantService.UpdateFeatureToggleAsync(
                         tenantId, featureId, body.IsEnabled);
            return ok ? NoContent() : BadRequest("Feature not found or parent OFF.");
        }
    }

    /// <summary>Simple body for the toggle endpoints.</summary>
    public record ToggleDto(bool IsEnabled);
}
