using mylittle_project.Application.DTOs;

namespace mylittle_project.Application.Interfaces
{
    public interface IProductInterface
    {
        Task<Guid> CreateSectionAsync(ProductCreateDto dto);
        Task<Guid> CreateFieldAsync(ProductFieldDto dto);

        Task<bool> UpdateSectionAsync(Guid id, ProductCreateDto dto);
        Task<bool> UpdateFieldAsync(Guid id, ProductFieldDto dto);

        Task<bool> DeleteSectionAsync(Guid id);
        Task<bool> DeleteFieldAsync(Guid id);

        Task<List<ProductSectionDto>> GetAllSectionsWithFieldsAsync();

        Task<List<ProductSectionDto>> GetDealerVisibleSectionsAsync();
    }
}
