using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;

namespace mylittle_project.infrastructure.Services
{
    public class DealerSubscriptionApplicationService : IDealerSubscriptionApplicationService
    {
        private readonly AppDbContext _context;

        public DealerSubscriptionApplicationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddSubscriptionAsync(DealerSubscriptionApplicationDto dto)
        {
            // Check for existing application (prevent duplicates)
            var existing = await _context.DealerSubscriptionApplications
                .FirstOrDefaultAsync(x =>
                    x.DealerId == dto.DealerId &&
                    x.TenantId == dto.TenantId &&
                    x.CategoryId == dto.CategoryId &&
                    x.PlanType == dto.PlanType);

            if (existing != null)
            {
                return (false, "Dealer has already applied for this category and plan.");
            }

            // Insert new subscription application
            var subscription = new DealerSubscriptionApplication
            {
                Id = Guid.NewGuid(),
                DealerId = dto.DealerId,
                TenantId = dto.TenantId,
                CategoryId = dto.CategoryId,
                PlanType = dto.PlanType,
                StartDate = dto.StartDate,
                IsQueued = false,
                Status = "Pending"  // You can customize this later if needed
            };

            _context.DealerSubscriptionApplications.Add(subscription);
            await _context.SaveChangesAsync();

            return (true, "Subscription application submitted successfully.");
        }

        public async Task<List<DealerSubscriptionApplicationDto>> GetByTenantAsync(Guid tenantId)
        {
            return await _context.DealerSubscriptionApplications
                .Where(x => x.TenantId == tenantId)
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
