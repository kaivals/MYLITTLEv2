using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.infrastructure.Data;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System.Linq;
using System.Security.Claims;

namespace mylittle_project.Infrastructure.Services
{
    public class FilterService : IFilterService
    {
        private readonly AppDbContext _context;
        private readonly IFeatureAccessService _featureAccess;
        private readonly IHttpContextAccessor _httpContext;

        public FilterService(AppDbContext context, IFeatureAccessService featureAccess, IHttpContextAccessor httpContext)
        {
            _context = context;
            _featureAccess = featureAccess;
            _httpContext = httpContext;
        }

        public async Task<List<FilterDto>> GetAllAsync()
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "filters");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Filters feature not enabled for this tenant.");

            return await _context.Filters
                .Where(f => f.TenantId == tenantId)
                .Select(f => new FilterDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Values = f.Values
                }).ToListAsync();
        }

        public async Task<PaginatedResult<FilterDto>> GetPaginatedAsync(int page, int pageSize)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "filters");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Filters feature not enabled for this tenant.");

            var query = _context.Filters
                .Where(f => f.TenantId == tenantId)
                .Select(f => new FilterDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Values = f.Values
                });

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<FilterDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<FilterDto> GetByIdAsync(Guid id)
        {
            var tenantId = GetTenantId();
            var f = await _context.Filters.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);
            if (f == null) return null;

            return new FilterDto
            {
                Id = f.Id,
                Name = f.Name,
                Values = f.Values
            };
        }

        public async Task<FilterDto> CreateAsync(CreateFilterDto dto)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "filters");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Filters feature not enabled for this tenant.");

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Values = dto.Values
            };

            _context.Filters.Add(filter);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(filter.Id);
        }

        public async Task<FilterDto> UpdateAsync(Guid id, CreateFilterDto dto)
        {
            var tenantId = GetTenantId();
            var f = await _context.Filters.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);
            if (f == null) return null;

            f.Name = dto.Name;
            f.Values = dto.Values;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var tenantId = GetTenantId();
            var f = await _context.Filters.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);
            if (f == null) return false;

            _context.Filters.Remove(f);
            await _context.SaveChangesAsync();
            return true;
        }

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }
    }
}
