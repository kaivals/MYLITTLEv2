using MyProject.Application.DTOs;
using MyProject.Application.DTOs.Common;
using System.Threading.Tasks;

namespace MyProject.Application.Interfaces.Common
{
    public interface IBaseFilterService<TDto, in TFilterDto>
        where TFilterDto : BaseFilterDto
    {
        Task<PaginatedResult<TDto>> GetFilteredAsync(TFilterDto filter);
    }
}
