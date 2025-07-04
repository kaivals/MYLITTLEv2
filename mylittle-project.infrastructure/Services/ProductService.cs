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
    public class ProductService : IProductService
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

            var fieldId = Guid.NewGuid();

            var field = new ProductField
            {
                Id = fieldId,
                TenantId = tenantId,
                SectionId = dto.SectionId,
                Name = dto.Name,
                FieldType = dto.FieldType,
                IsVisibleToDealer = dto.IsVisibleToDealer,
                IsRequired = dto.IsRequired,
                AutoSyncEnabled = dto.AutoSyncEnabled,
                IsFilterable = dto.IsFilterable,
                IsVariantOption = dto.IsVariantOption,
                IsVisible = dto.IsVisible,
                Options = dto.Options,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductFields.AddAsync(field);

                // Auto-sync logic to ProductAttribute
                if (field.AutoSyncEnabled)
                {
                    var attribute = new ProductAttribute
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Name = field.Name,
                        FieldType = field.FieldType,
                        Options = field.Options != null ? string.Join(",", field.Options) : null,
                        IsRequired = field.IsRequired,
                        IsVisible = true,
                        Source = "AutoSync",
                        SectionType = "Info",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ProductAttributes.AddAsync(attribute);
                }

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return fieldId;
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
            field.IsVisibleToDealer = dto.IsVisibleToDealer;
            field.IsRequired = dto.IsRequired;
            field.AutoSyncEnabled = dto.AutoSyncEnabled;
            field.IsFilterable = dto.IsFilterable;
            field.IsVariantOption = dto.IsVariantOption;
            field.IsVisible = dto.IsVisible;
            field.Options = dto.Options;
            field.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.ProductFields.Update(field);

                if (field.AutoSyncEnabled)
                {
                    var attribute = await _unitOfWork.ProductAttributes
                        .GetAll()
                        .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Name == field.Name);

                    if (attribute != null)
                    {
                        attribute.FieldType = field.FieldType;
                        attribute.Options = field.Options != null ? string.Join(",", field.Options) : null;
                        attribute.IsRequired = field.IsRequired;
                        attribute.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.ProductAttributes.Update(attribute);
                    }
                    else
                    {
                        var newAttribute = new ProductAttribute
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            Name = field.Name,
                            FieldType = field.FieldType,
                            Options = field.Options != null ? string.Join(",", field.Options) : null,
                            IsRequired = field.IsRequired,
                            IsVisible = true,
                            Source = "AutoSync",
                            SectionType = "Info",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.ProductAttributes.AddAsync(newAttribute);
                    }
                }

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
                    SectionId = s.Id,
                    IsVisibleToDealer = f.IsVisibleToDealer,
                    IsRequired = f.IsRequired,
                    AutoSyncEnabled = f.AutoSyncEnabled,
                    IsFilterable = f.IsFilterable,
                    IsVariantOption = f.IsVariantOption,
                    IsVisible = f.IsVisible,
                    Options = f.Options
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
                        .Where(f => f.IsVisibleToDealer)
                        .Select(f => new ProductFieldDto
                        {
                            Id = f.Id,
                            Name = f.Name,
                            FieldType = f.FieldType,
                            SectionId = s.Id,
                            IsVisibleToDealer = f.IsVisibleToDealer,
                            IsRequired = f.IsRequired,
                            AutoSyncEnabled = f.AutoSyncEnabled,
                            IsFilterable = f.IsFilterable,
                            IsVariantOption = f.IsVariantOption,
                            IsVisible = f.IsVisible,
                            Options = f.Options
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}
