using MyProject.Application.DTOs;

namespace MyProject.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<PaginatedResult<CategoryDto>> GetAllPaginatedAsync(int page, int pageSize);
        Task<CategoryDto> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto);
        Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
