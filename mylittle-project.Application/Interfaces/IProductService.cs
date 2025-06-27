using mylittle_project.Application.DTOs;
namespace mylittle_project.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> GetByIdAsync(Guid id);
        Task CreateAsync(ProductDto product);
        Task UpdateAsync(Guid id, ProductDto product);
        Task DeleteAsync(Guid id);

        Task<PaginatedResult<ProductDto>> GetPaginatedAsync(int page, int pageSize);

    }
}
