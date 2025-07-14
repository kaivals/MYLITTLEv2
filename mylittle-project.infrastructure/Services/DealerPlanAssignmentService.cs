using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.infrastructure.Services
{
    public class DealerPlanAssignmentService : IDealerPlanAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DealerPlanAssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DealerPlanAssignment>> GetByTenantAsync(Guid tenantId)
        {
            return await _unitOfWork.TenantPlanAssignments.Find(x => x.TenantId == tenantId)
                .Include(x => x.Category)
                .Include(x => x.Dealer)
                .ToListAsync();
        }

        public async Task<List<SchedulerAssignmentDto>> GetSchedulerAssignmentsAsync(Guid tenantId)
        {
            var assignments = await _unitOfWork.TenantPlanAssignments.Find(x => x.TenantId == tenantId && !x.IsDeleted)
                .Include(x => x.Category)
                .Include(x => x.Dealer)
                .ToListAsync();

            return assignments.Select(x => new SchedulerAssignmentDto
            {
                Category = x.Category?.Name ?? "N/A",
                Dealer = x.Dealer?.DealerName ?? "N/A",
                PlanType = x.PlanType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = GetSubscriptionStatus(x.StartDate, x.EndDate)
            }).ToList();
        }

        private string GetSubscriptionStatus(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            if (endDate < today) return "Expired";
            if (startDate > today) return "Upcoming";
            return "Active";
        }

        public async Task<(bool Success, List<string> Errors)> AddAssignmentsAsync(Guid tenantId, List<DealerPlanAssignmentDto> dtos)
        {
            var tenantSubscription = await _unitOfWork.TenantSubscriptions
                .Find(t => t.TenantId == tenantId && t.IsActive)
                .FirstOrDefaultAsync();

            if (tenantSubscription == null)
                return (false, new List<string> { "Active Tenant Subscription not found." });

            int maxSlotsAllowed = tenantSubscription.MaxMembers;

            int assignedDealersCount = await _unitOfWork.TenantPlanAssignments
                .Find(x => x.TenantId == tenantId)
                .Select(x => x.DealerId)
                .Distinct()
                .CountAsync();

            var allowedPlans = await _unitOfWork.TenantSubscriptions
                .Find(t => t.TenantId == tenantId && t.IsActive)
                .Select(t => t.PlanName)
                .ToListAsync();

            List<string> errors = new();

            foreach (var dto in dtos)
            {
                if (!allowedPlans.Contains(dto.PlanType))
                {
                    errors.Add($"Plan '{dto.PlanType}' is not allowed for Dealer {dto.DealerId}.");
                    continue;
                }

                if (assignedDealersCount >= maxSlotsAllowed)
                {
                    errors.Add($"Max slots reached. Cannot assign Dealer {dto.DealerId}.");
                    continue;
                }

                var application = await _unitOfWork.DealerSubscriptions
                    .Find(a => a.DealerId == dto.DealerId && a.TenantId == tenantId && a.CategoryId == dto.CategoryId && a.PlanType == dto.PlanType)
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    errors.Add($"Dealer {dto.DealerId} has not applied for Category {dto.CategoryId} & Plan '{dto.PlanType}'.");
                    continue;
                }

                var existingAssignment = await _unitOfWork.TenantPlanAssignments
                    .Find(x => x.TenantId == tenantId && x.DealerId == dto.DealerId && x.CategoryId == dto.CategoryId && x.PlanType == dto.PlanType)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null)
                {
                    errors.Add($"Plan already assigned to Dealer {dto.DealerId} for Category {dto.CategoryId} & Plan '{dto.PlanType}'.");
                    continue;
                }

                _unitOfWork.TenantPlanAssignments.Add(new DealerPlanAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CategoryId = application.CategoryId,
                    DealerId = dto.DealerId,
                    PlanType = dto.PlanType,
                    StartDate = application.StartDate,
                    EndDate = application.StartDate.AddMonths(12),
                    Status = dto.Status,
                    SlotsUsed = dto.SlotsUsed,
                    MaxSlots = dto.MaxSlots
                });

                application.Status = "Active";
                assignedDealersCount++;
            }

            await _unitOfWork.SaveAsync();
            bool success = errors.Count == 0;
            return (success, errors);
        }

        public async Task<bool> UpdateAssignmentAsync(Guid id, DealerPlanAssignmentDto dto)
        {
            var existing = await _unitOfWork.TenantPlanAssignments.GetByIdAsync(id);
            if (existing == null) return false;

            existing.CategoryId = dto.CategoryId;
            existing.DealerId = dto.DealerId;
            existing.PlanType = dto.PlanType;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.Status = dto.Status;
            existing.SlotsUsed = dto.SlotsUsed;
            existing.MaxSlots = dto.MaxSlots;

            _unitOfWork.TenantPlanAssignments.Update(existing);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAssignmentAsync(Guid id)
        {
            var existing = await _unitOfWork.TenantPlanAssignments.GetByIdAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;

            _unitOfWork.TenantPlanAssignments.Update(existing);
            await _unitOfWork.SaveAsync();
            return true;
        }

    }
}
