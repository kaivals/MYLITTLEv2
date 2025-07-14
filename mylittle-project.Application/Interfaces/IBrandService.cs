using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using mylittle_project.Application.DTOs;

namespace mylittle_project.Application.Interfaces
{
    public interface IBrandService
    {
        Task<List<BrandProductDto>> GetAllAsync();
        Task<BrandProductDto?> GetByIdAsync(Guid id);
        Task<BrandProductDto> CreateAsync(CreateBrandDto dto);
        Task<BrandProductDto?> UpdateAsync(Guid id, UpdateBrandDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteBrandAsync(Guid brandId);
    }
}
