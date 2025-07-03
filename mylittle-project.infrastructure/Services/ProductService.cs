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
    public class ProductService : IProductInterface
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureAccessService _featureAccessService;
        private readonly IHttpContextAccessor _httpContext;

        public ProductService(
            IUnitOfWork unitOfWork,
            IFeatureAccessService featureAccessService,
            IHttpContextAccessor httpContext)
        {
            _unitOfWork = unitOfWork;
            _featureAccessService = featureAccessService;
            _httpContext = httpContext;
        }

        private Guid GetTenantId()
        {
            var tenantId = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantId == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantId);
        }

        public async Task<Guid> CreateSectionAsync(ProductCreateDto dto)
        {
            var tenantId = GetTenantId();

            if (!await _featureAccessService.IsFeatureEnabledAsync(tenantId, "products"))
                throw new UnauthorizedAccessException("Product feature not enabled for this tenant.");

            var section = new ProductSection
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductSections.AddAsync(section);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return section.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Guid> CreateFieldAsync(ProductFieldDto dto)
        {
            var tenantId = GetTenantId();

            if (!await _featureAccessService.IsFeatureEnabledAsync(tenantId, "products"))
                throw new UnauthorizedAccessException("Product feature not enabled for this tenant.");

            var field = new ProductField
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SectionId = dto.SectionId,
                Name = dto.Name,
                FieldType = dto.FieldType,
                IsVisibleToDealer = dto.VisibleToDealer,
                IsRequired = dto.IsRequired,
                AutoSyncEnabled = dto.AutoSyncEnabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductFields.AddAsync(field);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return field.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateSectionAsync(Guid id, ProductCreateDto dto)
        {
            var tenantId = GetTenantId();

            var section = await _unitOfWork.ProductSections
                .GetAll()
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (section == null) return false;

            section.Name = dto.Name;
            section.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductSections.Update(section);
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

        public async Task<bool> UpdateFieldAsync(Guid id, ProductFieldDto dto)
        {
            var tenantId = GetTenantId();

            var field = await _unitOfWork.ProductFields
                .GetAll()
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (field == null) return false;

            field.Name = dto.Name;
            field.FieldType = dto.FieldType;
            field.SectionId = dto.SectionId;
            field.IsVisibleToDealer = dto.VisibleToDealer;
            field.IsRequired = dto.IsRequired;
            field.AutoSyncEnabled = dto.AutoSyncEnabled;
            field.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductFields.Update(field);
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

        public async Task<bool> DeleteSectionAsync(Guid id)
        {
            var tenantId = GetTenantId();

            var section = await _unitOfWork.ProductSections
                .GetAll()
                .Include(s => s.Fields)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (section == null) return false;

            if (section.Fields.Any())
                throw new InvalidOperationException("Cannot delete section with existing fields.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductSections.Remove(section);
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

        public async Task<bool> DeleteFieldAsync(Guid id)
        {
            var tenantId = GetTenantId();

            var field = await _unitOfWork.ProductFields
                .GetAll()
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (field == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductFields.Remove(field);
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

        public async Task<List<ProductSectionDto>> GetAllSectionsWithFieldsAsync()
        {
            var tenantId = GetTenantId();

            var sections = await _unitOfWork.ProductSections
                .GetAll()
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Fields)
                .ToListAsync();

            return sections.Select(s => new ProductSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                Fields = s.Fields.Select(f => new ProductFieldDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    FieldType = f.FieldType,
                    IsVisibleToDealer = f.IsVisibleToDealer,
                    IsRequired = f.IsRequired,
                    AutoSyncEnabled = f.AutoSyncEnabled
                }).ToList()
            }).ToList();
        }


        public async Task<List<ProductSectionDto>> GetDealerVisibleSectionsAsync()
        {
            var tenantId = GetTenantId();

            var sections = await _unitOfWork.ProductSections
                .GetAll()
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Fields)
                .ToListAsync();

            return sections
                .Select(s => new ProductSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Fields = s.Fields
                        .Where(f => f.IsVisibleToDealer) // ✅ Only dealer-visible fields
                        .Select(f => new ProductFieldDto
                        {
                            Id = f.Id,
                            Name = f.Name,
                            FieldType = f.FieldType,
                            IsVisibleToDealer = f.IsVisibleToDealer,
                            IsRequired = f.IsRequired,
                            AutoSyncEnabled = f.AutoSyncEnabled
                        })
                        .ToList()
                })
                .ToList();
        }












    }
}
