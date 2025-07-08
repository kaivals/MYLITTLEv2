using mylittle_project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IDealerService
    {
        Task<Guid> CreateBusinessInfoAsync(DealerDto dto);
        Task<List<object>> GetAllDealersAsync();                // For Admin / Owner
        Task<List<object>> GetDealersByTenantAsync(Guid tenantId); // For Specific Tenant
    }
}
