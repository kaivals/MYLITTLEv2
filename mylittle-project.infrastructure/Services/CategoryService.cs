using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
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
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Include(c => c.Filters)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    ParentName = c.Parent != null ? c.Parent.Name : null,
                    ProductCount = c.Products.Count,
                    FilterCount = c.Filters.Count,
                    Status = c.Status,
                    Created = c.Created,
                    Updated = c.Updated,
                    AssignedFilters = c.AssignedFilters.Select(f => new AssignedFilterDto
                    {
                        Name = f.Name,
                        Values = f.Values
                    }).ToList()
                })
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
            var c = await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Include(c => c.Filters)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return null;

            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ParentId = c.ParentId,
                ParentName = c.Parent?.Name,
                ProductCount = c.Products.Count,
                FilterCount = c.Filters.Count,
                Status = c.Status,
                Created = c.Created,
                Updated = c.Updated,
                AssignedFilters = c.AssignedFilters.Select(f => new AssignedFilterDto
                {
                    Name = f.Name,
                    Values = f.Values
                }).ToList()
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

            var validFilterMap = await _context.Filters
                .Where(f => f.TenantId == tenantId)
                .GroupBy(f => f.Name)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.SelectMany(f => f.Values).Distinct().ToList()
                );

            var filteredAssigned = dto.AssignedFilters
                  .Where(f => validFilterMap.ContainsKey(f.Name))
                  .Select(f => new AssignedFilter
                  {
                      Name = f.Name,
                      Values = f.Values
                          .Where(v => validFilterMap[f.Name].Contains(v))
                          .ToList()
                  })
                  .Where(f => f.Values.Any()) // ensure only filters with valid values are saved
                  .ToList();

            var category = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                ParentId = dto.ParentId,
                Status = dto.Status,
                ProductCount = 0,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                AssignedFilters = filteredAssigned
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(category.Id);
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return null;

            var tenantId = GetTenantId();

            var validFilterMap = await _context.Filters
                .Where(f => f.TenantId == tenantId)
                .GroupBy(f => f.Name)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.SelectMany(f => f.Values).Distinct().ToList()
                );

            var filteredAssigned = dto.AssignedFilters
                    .Where(f => validFilterMap.ContainsKey(f.Name))
                    .Select(f => new AssignedFilter
                    {
                        Name = f.Name,
                        Values = f.Values
                            .Where(v => validFilterMap[f.Name].Contains(v))
                            .ToList()
                    })
                    .Where(f => f.Values.Any()) // ensure only filters with valid values are saved
                    .ToList();


            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.ParentId = dto.ParentId;
            category.Status = dto.Status;
            category.AssignedFilters = filteredAssigned;
            category.Updated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResult<CategoryDto>> GetFilteredAsync(CategoryFilterDto filter)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

            var query = _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Include(c => c.Filters)
                .Where(c => c.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(c => c.Name.Contains(filter.Name));

            if (filter.ParentId.HasValue)
                query = query.Where(c => c.ParentId == filter.ParentId.Value);

            if (filter.HasProducts.HasValue)
                query = query.Where(c => c.Products.Any() == filter.HasProducts.Value);

            if (filter.HasFilters.HasValue)
                query = query.Where(c => c.Filters.Any() == filter.HasFilters.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(c => c.Status == filter.Status);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(c => c.Created >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(c => c.Created <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection == "asc" ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "productcount" => filter.SortDirection == "asc" ? query.OrderBy(c => c.Products.Count) : query.OrderByDescending(c => c.Products.Count),
                "filtercount" => filter.SortDirection == "asc" ? query.OrderBy(c => c.Filters.Count) : query.OrderByDescending(c => c.Filters.Count),
                _ => filter.SortDirection == "asc" ? query.OrderBy(c => c.Created) : query.OrderByDescending(c => c.Created)
            };

            var totalItems = await query.CountAsync();
            var items = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsEnumerable() // switch to client-side projection
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    ParentName = c.Parent?.Name,
                    ProductCount = c.Products.Count,
                    FilterCount = c.Filters.Count,
                    Status = c.Status,
                    Created = c.Created,
                    Updated = c.Updated,
                    AssignedFilters = c.AssignedFilters.Select(f => new AssignedFilterDto
                    {
                        Name = f.Name,
                        Values = f.Values
                    }).ToList()
                })
                .ToList(); // ✅ Correct method here



            return new PaginatedResult<CategoryDto>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems
            };
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
