﻿using mylittle_project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IDealerSubscriptionApplicationService
    {
        Task<(bool Success, string Message)> AddSubscriptionAsync(DealerSubscriptionApplicationDto dto);
        Task<List<DealerSubscriptionApplicationDto>> GetByTenantAsync(Guid tenantId);
        Task<List<DealerSubscriptionApplicationDto>> GetByDealerAsync(Guid dealerId);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> RestoreAsync(Guid subscriptionId);
    }
}
