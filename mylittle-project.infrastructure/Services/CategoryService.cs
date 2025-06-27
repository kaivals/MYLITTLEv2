using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using MyProject.Application.DTOs;
using MyProject.Application.DTOs.Common;
using MyProject.Application.Interfaces;
using MyProject.Application.Interfaces.Common;
using MyProject.Infrastructure.Services.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
{
    public class CategoryService : BaseFilterService<Category, CategoryDto, BaseFilterDto>, ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IFeatureAccessService _featureAccess;
        private readonly IHttpContextAccessor _httpContext;

        public CategoryService(AppDbContext context, IFeatureAccessService featureAccess, IHttpContextAccessor httpContext)
            : base(context)
        {
            _context = context;
            _featureAccess = featureAccess;
            _httpContext = httpContext;
        }

        protected override IQueryable<Category> ApplyBaseFilters(IQueryable<Category> query, BaseFilterDto filter)
        {
            var tenantId = GetTenantId();
            query = query.Where(c => c.TenantId == tenantId)
                         .Include(c => c.Parent)
                         .Include(c => c.Products)
                         .Include(c => c.Filters);

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

            if (filter.DateFilterValue1.HasValue)
            {
                var date = filter.DateFilterValue1.Value;
                switch (filter.DateFilterOperator)
                {
                    case "Is after":
                        query = query.Where(c => c.Created > date); break;
                    case "Is after or equal to":
                        query = query.Where(c => c.Created >= date); break;
                    case "Is before":
                        query = query.Where(c => c.Created < date); break;
                    case "Is before or equal to":
                        query = query.Where(c => c.Created <= date); break;
                    case "Is equal to":
                        query = query.Where(c => c.Created.Date == date.Date); break;
                    case "Is not equal to":
                        query = query.Where(c => c.Created.Date != date.Date); break;
                }
            }

            if (filter.DateFilterValue1.HasValue && filter.DateFilterValue2.HasValue && filter.DateFilterOperator == "between")
            {
                query = query.Where(c => c.Created >= filter.DateFilterValue1 && c.Created <= filter.DateFilterValue2);
            }

            return query;
        }

        protected override Expression<Func<Category, CategoryDto>> MapToDto()
        {
            return c => new CategoryDto
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
                Updated = c.Updated
            };
        }

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
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
                Updated = c.Updated
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "categories");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Category feature not enabled for this tenant.");

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
                Updated = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(category.Id);
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return null;

            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.ParentId = dto.ParentId;
            category.Status = dto.Status;
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

        public Task<PaginatedResult<CategoryDto>> GetAllPaginatedAsync(int page, int pageSize)
        {
            var filter = new BaseFilterDto
            {
                Page = 1,      // static fixed page number
                PageSize = 20  // static fixed page size
            };

            return GetFilteredAsync(filter);
        }
    }
}
