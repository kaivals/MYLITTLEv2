using System;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Application.DTOs.Common;

namespace MyProject.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<PaginatedResult<CategoryDto>> GetAllPaginatedAsync(int page, int pageSize);
        Task<PaginatedResult<CategoryDto>> GetFilteredAsync(BaseFilterDto filter);
        Task<CategoryDto> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto);
        Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
