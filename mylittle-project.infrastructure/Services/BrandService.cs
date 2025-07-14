using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.Infrastructure.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IFeatureAccessService _featureAccess;

        public BrandService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContext, IFeatureAccessService featureAccess)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContext;
            _featureAccess = featureAccess;
        }

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (string.IsNullOrEmpty(tenantIdHeader))
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }

        private async Task EnsureFeatureEnabledAsync()
        {
            var tenantId = GetTenantId();
            var isEnabled = await _featureAccess.IsFeatureEnabledAsync(tenantId, "brands");
            if (!isEnabled)
                throw new UnauthorizedAccessException("Brand feature not enabled for this tenant.");
        }

        public async Task<List<BrandProductDto>> GetAllAsync()
        {
            await EnsureFeatureEnabledAsync();

            var brands = await _unitOfWork.Brands.Find(b => !b.IsDeleted).ToListAsync();

            return brands.Select(b => new BrandProductDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Status = b.Status,
                Order = b.Order,
                Created = b.CreatedAt
            }).ToList();
        }

        public async Task<BrandProductDto?> GetByIdAsync(Guid id)
        {
            await EnsureFeatureEnabledAsync();

            var brand = await _unitOfWork.Brands.Find(b => b.Id == id && !b.IsDeleted).FirstOrDefaultAsync();
            if (brand == null) return null;

            return new BrandProductDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Status = brand.Status,
                Order = brand.Order,
                Created = brand.CreatedAt
            };
        }

        public async Task<BrandProductDto> CreateAsync(CreateBrandDto dto)
        {
            await EnsureFeatureEnabledAsync();

            var brand = new BrandProduct
            {
                Id = Guid.NewGuid(),
                TenantId = GetTenantId(),
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                Order = dto.Order,
                LogoUrl = dto.LogoUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Brands.AddAsync(brand);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new BrandProductDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Status = brand.Status,
                Order = brand.Order,
                TenantId = brand.TenantId,
                LogoUrl = brand.LogoUrl,
                Created = brand.CreatedAt
            };
        }

        public async Task<BrandProductDto?> UpdateAsync(Guid id, UpdateBrandDto dto)
        {
            await EnsureFeatureEnabledAsync();

            var brand = await _unitOfWork.Brands.Find(b => b.Id == id && !b.IsDeleted).FirstOrDefaultAsync();
            if (brand == null) return null;

            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.Status = dto.Status;
            brand.Order = dto.Order;
            brand.LogoUrl = dto.LogoUrl;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _unitOfWork.Brands.Update(brand);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new BrandProductDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Status = brand.Status,
                Order = brand.Order,
                TenantId = brand.TenantId,
                LogoUrl = brand.LogoUrl,
                Created = brand.CreatedAt
            };
        }

        [Obsolete("Use SoftDeleteBrandAsync instead.")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null) return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _unitOfWork.Brands.Remove(brand);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteBrandAsync(Guid brandId)
        {
            var brand = await _unitOfWork.Brands.Find(b => b.Id == brandId && !b.IsDeleted).FirstOrDefaultAsync();
            if (brand == null) return false;

            brand.IsDeleted = true;
            brand.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Brands.Update(brand);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return true;
        }
    }
}
