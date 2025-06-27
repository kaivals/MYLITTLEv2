using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mylittle_project.Application.DTOs;

namespace mylittle_project.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto dto);
        Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}

