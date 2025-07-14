using mylittle_project.Application.DTOs;
using mylittle_project.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IProductService
    {
        // Section Management
        Task<Guid> CreateSectionAsync(ProductSectionDto dto);

        // Field Management
        Task<Guid> CreateFieldAsync(ProductFieldDto dto);

        // Product Management
        Task<Guid> AddNewProductAsync(ProductDto dto);
        Task<ProductDetailsDto?> GetProductAsync(Guid id);
        Task<List<ProductFieldDto>> GetAllFieldsAsync();
        Task<List<Product>> FilterProductsAsync(ProductFilterRequest request);
        Task<bool> UpdateProductFieldsAsync(Guid productId, Dictionary<string, string> fieldValues);
        Task<int> ResyncProductFieldValuesAsync(Guid? tenantId = null);
        Task<bool> UpdateProductAsync(Guid productId, ProductDto dto);
        Task<bool> SoftDeleteProductFieldAsync(Guid fieldId);
        Task<bool> SoftDeleteProductSectionAsync(Guid sectionId);
    }
}
