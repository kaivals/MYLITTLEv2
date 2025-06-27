using mylittle_project.Application.DTOs;
using MyProject.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.Interfaces
{
    public interface IFilterService
    {
        Task<List<FilterDto>> GetAllAsync();
        Task<FilterDto> GetByIdAsync(Guid id);
        Task<FilterDto> CreateAsync(CreateFilterDto dto);
        Task<FilterDto> UpdateAsync(Guid id, CreateFilterDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}

