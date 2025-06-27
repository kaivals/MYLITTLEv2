using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using System.Security.Claims;

namespace MyProject.Infrastructure.Services
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

        public async Task<PaginatedResult<CategoryDto>> GetAllPaginatedAsync(int page, int pageSize)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

            var query = _context.Categories
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
                    AssignedFilters = c.AssignedFilters
                });

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<CategoryDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
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

            var validFilterNames = await _context.Filters
                .Where(f => f.TenantId == tenantId)
                .Select(f => f.Name)
                .ToListAsync();

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

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }
    }
}
