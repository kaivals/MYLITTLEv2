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
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.DealerSubscriptions.Add(subscription);
            await _unitOfWork.SaveAsync();

            return (true, "Subscription application submitted successfully.");
        }
        public async Task<bool> RestoreAsync(Guid subscriptionId)
        {
            var entity = await _unitOfWork.DealerSubscriptions.GetByIdAsync(subscriptionId);
            if (entity == null || !entity.IsDeleted)
                return false;

            entity.IsDeleted = false;
            await _unitOfWork.SaveAsync();
            return true;
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

        public async Task<List<DealerSubscriptionApplicationDto>> GetByDealerAsync(Guid dealerId)
        {
            return await _unitOfWork.DealerSubscriptions
                .Find(x => x.DealerId == dealerId)
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

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var subscription = await _unitOfWork.DealerSubscriptions.GetByIdAsync(id);
            if (subscription == null) return false;

            subscription.IsDeleted = true;
            subscription.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.DealerSubscriptions.Update(subscription);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
