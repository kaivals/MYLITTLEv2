using mylittle_project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface ITenantPortalLinkService
    {
        Task AddLinkAsync(TenantPortalLinkDto dto); // single
        Task AddLinksBatchAsync(TenantPortalLinkBatchDto dto); // multiple
        Task<IEnumerable<TenantPortalLinkViewDto>> GetAllLinkedPortalsAsync(); // view all links
        Task<IEnumerable<TenantDto>> GetAllTenantsAsync(); // corrected method

        Task<PaginatedResult<TenantPortalLinkDto>> GetPaginatedLinkedPortalsAsync(int page, int pageSize);

    }
}
