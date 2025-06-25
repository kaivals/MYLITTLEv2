using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mylittle_project.Application.DTOs;

namespace mylittle_project.Application.Interfaces
{
    public interface IBuyerService
    {
        Task<Guid> CreateBuyerAsync(BuyerCreateDto dto);
        Task<List<BuyerListDto>> GetAllBuyersAsync();
        Task<List<BuyerListDto>> GetBuyersByBusinessAsync(Guid businessId);
        Task<BuyerListDto?> GetBuyerByIdAsync(Guid id);
        Task<bool> SoftDeleteBuyerAsync(Guid buyerId);
        Task<bool> UpdateBuyerAsync(Guid buyerId, BuyerUpdateDto dto);

        Task<BuyerSummaryDto?> GetBuyerProfileAsync(Guid buyerId);
    
    }
}
