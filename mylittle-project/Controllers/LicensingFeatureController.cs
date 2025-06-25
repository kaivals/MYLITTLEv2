
//using Microsoft.AspNetCore.Mvc;
//using mylittle_project.Application.DTOs;
//using mylittle_project.Application.Interfaces;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace mylittle_project.API.Controllers
//{
  
//    [ApiController]
//    [Route("api/tenant-feature-settings")]
//    public class LicensingFeatureController : ControllerBase
//    {
//        private readonly ITenantService _tenantService;

//        public LicensingFeatureController(ITenantService tenantService)
//        {
//            _tenantService = tenantService;
//        }

//        // GET: api/tenant-feature-settings/tenants
//        [HttpGet("tenants")]
//        public async Task<IActionResult> GetAllTenants()
//        {
//            var tenants = await _tenantService.GetAllAsync();
//            var dto = tenants.Select(t => new
//            {
//                Id = t.Id,
//                TenantName = t.TenantName,
//                Subdomain = t.Subdomain,
//                IsActive = t.IsActive,
//                LastAccessed = t.LastAccessed
//            });

//            return Ok(dto);
//        }

//        // GET: api/tenant-feature-settings/{tenantId}
//        [HttpGet("{tenantId}")]
//        public async Task<IActionResult> GetFeatureSettings(Guid tenantId)
//        {
//            var settings = await _tenantService.GetFeatureTogglesAsync(tenantId);
//            if (settings == null)
//                return NotFound($"Feature settings not found for tenant {tenantId}");

//            return Ok(settings);
//        }

//        // PUT: api/tenant-feature-settings/{tenantId}
//        [HttpPut("{tenantId}")]
//        public async Task<IActionResult> UpdateFeatureSettings(Guid tenantId, [FromBody] FeatureSettingsDto dto)
//        {
//            if (tenantId != dto.TenantId)
//                return BadRequest("Tenant ID mismatch between route and body.");

//            var success = await _tenantService.UpdateFeatureTogglesAsync(tenantId, dto);
//            if (!success)
//                return NotFound("Tenant or feature settings not found.");

//            return Ok(new { message = "Feature settings updated successfully." });
//        }
//    }
//}
