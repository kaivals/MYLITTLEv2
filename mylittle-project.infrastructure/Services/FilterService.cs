using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.Domain.Enums;
using mylittle_project.infrastructure.Data;


namespace mylittle_project.Infrastructure.Services
{
    public class FilterService : IFilterService
    {
        private readonly AppDbContext _context;
        private readonly IFeatureAccessService _featureAccess;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUnitOfWork _unitOfWork;


            public FilterService(
                AppDbContext context,
                IFeatureAccessService featureAccess,
                IHttpContextAccessor httpContext,
                IUnitOfWork unitOfWork) // Add this
            {
                _context = context;
                _featureAccess = featureAccess;
                _httpContext = httpContext;
                _unitOfWork = unitOfWork; // Assign here
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
                    Type = f.Type,
                    IsDefault = f.IsDefault,
                    Description = f.Description,
                    Values = f.Values,
                    Status = f.Status,
                    Created = f.CreatedAt,
                    UsageCount = f.UsageCount,
                    LastModified = f.LastModified
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
                    Type = f.Type,
                    IsDefault = f.IsDefault,
                    Description = f.Description,
                    Values = f.Values,
                    Status = f.Status,
                    Created = f.CreatedAt,
                    UsageCount = f.UsageCount,
                    LastModified = f.LastModified
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
                Type = f.Type,
                IsDefault = f.IsDefault,
                Description = f.Description,
                Values = f.Values,
                Status = f.Status,
                Created = f.CreatedAt,
                UsageCount = f.UsageCount,
                LastModified = f.LastModified
            };
        }

        public async Task<FilterDto> CreateAsync(CreateFilterDto dto)
        {
            var tenantId = GetTenantId();
            var hasAccess = await _featureAccess.IsFeatureEnabledAsync(tenantId, "filters");
            if (!hasAccess)
                throw new UnauthorizedAccessException("Filters feature not enabled for this tenant.");

            ValidateFilterValues(dto);

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Type = dto.Type,
                IsDefault = dto.IsDefault,
                Description = dto.Description,
                Values = dto.Values,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
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

            ValidateFilterValues(dto);

            f.Name = dto.Name;
            f.Type = dto.Type;
            f.IsDefault = dto.IsDefault;
            f.Description = dto.Description;
            f.Values = dto.Values;
            f.Status = dto.Status;
            f.LastModified = DateTime.UtcNow;

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

        private void ValidateFilterValues(CreateFilterDto dto)
        {
            switch (dto.Type)
            {
                case FilterType.Dropdown:
                case FilterType.MultiSelect:
                    if (dto.Values == null || !dto.Values.Any())
                        throw new ArgumentException($"{dto.Type} filters must have at least one selectable value.");
                    break;

                case FilterType.Toggle:
                    if (dto.Values == null || dto.Values.Count != 2)
                        throw new ArgumentException("Toggle must have exactly two values (e.g., On/Off).");
                    break;

                case FilterType.Slider:
                    if (dto.Values == null || !dto.Values.All(v => int.TryParse(v, out _)))
                        throw new ArgumentException("Slider values must be numeric (e.g., 10,20,30).");
                    break;

                case FilterType.RangeSlider:
                    if (dto.Values == null || !dto.Values.All(v => v.Contains('-')))
                        throw new ArgumentException("RangeSlider values must be in 'min-max' format (e.g., 10-50).");
                    break;

                case FilterType.Text:
                    if (dto.Values != null && dto.Values.Any())
                        throw new ArgumentException("Text filters should not have predefined values.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(dto.Type), "Invalid filter type specified.");
            }
        }


        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenantid"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }
    }
}