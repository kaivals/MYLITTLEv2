using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class ProductTagService : IProductTagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureAccessService _featureAccess;
        private readonly IHttpContextAccessor _httpContext;

        public ProductTagService(IUnitOfWork unitOfWork, IFeatureAccessService featureAccess, IHttpContextAccessor httpContext)
        {
            _unitOfWork = unitOfWork;
            _featureAccess = featureAccess;
            _httpContext = httpContext;
        }

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");
            return Guid.Parse(tenantIdHeader);
        }

        private async Task EnsureFeatureEnabledAsync(Guid tenantId)
        {
            if (!await _featureAccess.IsFeatureEnabledAsync(tenantId, "product-tags"))
                throw new UnauthorizedAccessException("Product tag feature not enabled for this tenant.");
        }

        public async Task<List<ProductTagDto>> GetAllAsync()
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tags = await _unitOfWork.ProductTags
                .Find(t => !t.IsDeleted)  // ✅ Auto-filter soft-deleted tags
                .ToListAsync();

            return tags.Select(t => new ProductTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Published = t.Published,
                TaggedProducts = t.TaggedProducts,
                CreatedAt = t.Created
            }).ToList();
        }


        public async Task<ProductTagDto?> GetByIdAsync(Guid id)
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tag = await _unitOfWork.ProductTags.GetByIdAsync(id);
            return tag == null ? null : new ProductTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Published = tag.Published,
                TaggedProducts = tag.TaggedProducts,
                CreatedAt = tag.Created
            };
        }

        public async Task<ProductTagDto> CreateAsync(CreateProductTagDto dto)
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tag = new ProductTag
            {
                Name = dto.Name,
                Published = dto.Published,
                Created = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductTags.AddAsync(tag);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new ProductTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Published = tag.Published,
                TaggedProducts = tag.TaggedProducts,
                CreatedAt = tag.Created
            };
        }

        public async Task<ProductTagDto?> UpdateAsync(Guid id, UpdateProductTagDto dto)
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tag = await _unitOfWork.ProductTags.GetByIdAsync(id);
            if (tag == null) return null;

            tag.Name = dto.Name;
            tag.Published = dto.Published;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductTags.Update(tag);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new ProductTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Published = tag.Published,
                TaggedProducts = tag.TaggedProducts,
                CreatedAt = tag.Created
            };
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tag = await _unitOfWork.ProductTags.GetByIdAsync(id);
            if (tag == null || tag.IsDeleted) return false;

            tag.IsDeleted = true;
            tag.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductTags.Update(tag);
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





        public async Task<bool> BulkDeleteAsync(List<Guid> ids)
        {
            var tenantId = GetTenantId();
            await EnsureFeatureEnabledAsync(tenantId);

            var tags = _unitOfWork.ProductTags.Find(x => ids.Contains(x.Id) && !x.IsDeleted).ToList();
            if (tags.Count == 0) return false;

            foreach (var tag in tags)
            {
                tag.IsDeleted = true;
                tag.DeletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductTags.UpdateRange(tags);
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
    }
}