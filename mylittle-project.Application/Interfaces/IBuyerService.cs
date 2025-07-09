using mylittle_project.Application.DTOs;

public interface IBuyerService
{
    Task<Guid> CreateBuyerAsync(BuyerCreateDto dto);
    Task<List<BuyerListDto>> GetAllBuyersAsync(bool includePortal = false);  // ✅ Updated
    Task<List<BuyerListDto>> GetBuyersByBusinessAsync(Guid dealerId);
    Task<BuyerListDto?> GetBuyerByIdAsync(Guid id);
    Task<bool> SoftDeleteBuyerAsync(Guid buyerId);
    Task<bool> UpdateBuyerAsync(Guid buyerId, BuyerUpdateDto dto);
    Task<BuyerSummaryDto?> GetBuyerProfileAsync(Guid buyerId);
    Task<PaginatedResult<BuyerListDto>> GetAllBuyersPaginatedAsync(int page, int pageSize);
    Task<List<BuyerListDto>> GetBuyersForTenantManagerAsync(Guid tenantId, Guid? dealerId);
}
