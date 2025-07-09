using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.infrastructure.Services
{
    public class DealerSubscriptionApplicationService : IDealerSubscriptionApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DealerSubscriptionApplicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> AddSubscriptionAsync(DealerSubscriptionApplicationDto dto)
        {
            var existing = await _unitOfWork.DealerSubscriptions
                .Find(x => x.DealerId == dto.DealerId &&
                           x.TenantId == dto.TenantId &&
                           x.CategoryId == dto.CategoryId &&
                           x.PlanType == dto.PlanType)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return (false, "Dealer has already applied for this category and plan.");
            }

            var subscription = new DealerSubscriptionApplication
            {
                Id = Guid.NewGuid(),
                DealerId = dto.DealerId,
                TenantId = dto.TenantId,
                CategoryId = dto.CategoryId,
                PlanType = dto.PlanType,
                StartDate = dto.StartDate,
                IsQueued = false,
                Status = "Pending"
            };

            _unitOfWork.DealerSubscriptions.Add(subscription);
            await _unitOfWork.SaveAsync();

            return (true, "Subscription application submitted successfully.");
        }

        public async Task<List<DealerSubscriptionApplicationDto>> GetByTenantAsync(Guid tenantId)
        {
            return await _unitOfWork.DealerSubscriptions
                .Find(x => x.TenantId == tenantId)
                .Select(x => new DealerSubscriptionApplicationDto
                {
                    DealerId = x.DealerId,
                    TenantId = x.TenantId,
                    CategoryId = x.CategoryId,
                    PlanType = x.PlanType,
                    StartDate = x.StartDate,
                    IsQueued = x.IsQueued,
                    Status = x.Status
                })
                .ToListAsync();
        }
    }
}