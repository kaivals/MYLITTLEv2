using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using mylittle_project.Application.DTOs;
using System.Security.Claims;

namespace mylittle_project.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IFeatureAccessService _featureAccess;
        private readonly IHttpContextAccessor _httpContext;

        public CategoryService(AppDbContext context, IFeatureAccessService featureAccess, IHttpContextAccessor httpContext)
        {
            _context = context;
            _featureAccess = featureAccess;
            _httpContext = httpContext;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

            return await _context.Categories
                .Where(c => c.TenantId == tenantId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    Parent = c.Parent,
                    ProductCount = c.ProductCount,
                    Status = c.Status,
                    Created = c.Created,
                    Updated = c.Updated,
                    AssignedFilters = c.AssignedFilters // ✅ This includes filter names
                })
                .ToListAsync();
        }


        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return null;

            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                Parent = c.Parent,
                ProductCount = c.ProductCount,
                Status = c.Status,
                Created = c.Created,
                Updated = c.Updated,
                AssignedFilters = c.AssignedFilters
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

            // ✅ Fetch all valid filter names for this tenant
            var validFilterNames = await _context.Filters
                .Where(f => f.TenantId == tenantId)
                .Select(f => f.Name)
                .ToListAsync();

            // ✅ Keep only valid filters
            var filteredAssigned = dto.AssignedFilters
                .Where(f => validFilterNames.Contains(f))
                .ToList();

            var now = DateTime.UtcNow;
            var category = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                Parent = dto.Parent,
                Status = dto.Status,
                ProductCount = 0,
                Created = now,
                Updated = now,
                AssignedFilters = filteredAssigned
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(category.Id);
        }


        public async Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return null;

            c.Name = dto.Name;
            c.Slug = dto.Slug;
            c.Description = dto.Description;
            c.Parent = dto.Parent;
            c.Status = dto.Status;
            c.Updated = DateTime.UtcNow;
            c.AssignedFilters = dto.AssignedFilters;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return false;

            _context.Categories.Remove(c);
            await _context.SaveChangesAsync();
            return true;
        }

        // 🔧 New method: enable/disable "categories" and "filters" for a tenant
        public async Task SetCategoryFeaturesAsync(Guid tenantId, bool isCategoryEnabled)
        {
            var featureKeys = new[] { "categories", "filters" };

            var features = await _context.Features
                .Where(f => featureKeys.Contains(f.Key))
                .ToListAsync();

            foreach (var feature in features)
            {
                var existing = await _context.TenantFeatures
                    .FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FeatureId == feature.Id);

                if (existing != null)
                {
                    existing.IsEnabled = isCategoryEnabled;
                }
                else
                {
                    _context.TenantFeatures.Add(new TenantFeature
                    {
                        TenantId = tenantId,
                        FeatureId = feature.Id,
                        IsEnabled = isCategoryEnabled,
                        ModuleId = feature.ModuleId
                    });

                }
            }

            await _context.SaveChangesAsync();
        }

        //private Guid GetTenantId()
        //{
        //    return Guid.Parse("D4729C24-B27A-429C-ACDF-2E6418C18975"); // your test tenant
        //}


        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }




    }
}
