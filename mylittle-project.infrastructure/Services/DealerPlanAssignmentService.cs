using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
{
    public class DealerPlanAssignmentService : IDealerPlanAssignmentService
    {
        private readonly AppDbContext _context;

        public DealerPlanAssignmentService(AppDbContext context)
        {
            _context = context;
        }

            public async Task<List<DealerPlanAssignment>> GetByTenantAsync(Guid tenantId)
            {
                return await _context.TenantPlanAssignments
                    .Include(x => x.Category)
                    .Include(x => x.Dealer)
                    .Where(x => x.TenantId == tenantId)
                    .ToListAsync();
            }
        public async Task<List<SchedulerAssignmentDto>> GetSchedulerAssignmentsAsync(Guid tenantId)
        {
            var assignments = await _context.TenantPlanAssignments
                .Include(x => x.Category)
                .Include(x => x.Dealer)
                .Where(x => x.TenantId == tenantId && !x.IsDeleted) // If soft-delete exists
                .ToListAsync();

            var result = assignments.Select(x => new SchedulerAssignmentDto
            {
                Category = x.Category?.Name ?? "N/A",
                Dealer = x.Dealer?.DealerName ?? "N/A",
                PlanType = x.PlanType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = GetSubscriptionStatus(x.StartDate, x.EndDate)
            }).ToList();

            return result;
        }

        private string GetSubscriptionStatus(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            if (endDate < today)
                return "Expired";
            if (startDate > today)
                return "Upcoming";
            return "Active";
        }

        public async Task<(bool Success, List<string> Errors)> AddAssignmentsAsync(Guid tenantId, List<DealerPlanAssignmentDto> dtos)
        {
            var tenantSubscription = await _context.TenantSubscriptions
                .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsActive);

            if (tenantSubscription == null)
                return (false, new List<string> { "Active Tenant Subscription not found." });

            int maxSlotsAllowed = tenantSubscription.MaxMembers;

            int assignedDealersCount = await _context.TenantPlanAssignments
                .Where(x => x.TenantId == tenantId)
                .Select(x => x.DealerId)
                .Distinct()
                .CountAsync();

            var allowedPlans = await _context.TenantSubscriptions
                .Where(t => t.TenantId == tenantId && t.IsActive)
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

                var application = await _context.DealerSubscriptionApplications
                    .FirstOrDefaultAsync(a =>
                        a.DealerId == dto.DealerId &&
                        a.TenantId == tenantId &&
                        a.CategoryId == dto.CategoryId &&
                        a.PlanType == dto.PlanType);

                if (application == null)
                {
                    errors.Add($"Dealer {dto.DealerId} has not applied for Category {dto.CategoryId} & Plan '{dto.PlanType}'.");
                    continue;
                }

                var existingAssignment = await _context.TenantPlanAssignments
                    .FirstOrDefaultAsync(x =>
                        x.TenantId == tenantId &&
                        x.DealerId == dto.DealerId &&
                        x.CategoryId == dto.CategoryId &&
                        x.PlanType == dto.PlanType);

                if (existingAssignment != null)
                {
                    errors.Add($"Plan already assigned to Dealer {dto.DealerId} for Category {dto.CategoryId} & Plan '{dto.PlanType}'.");
                    continue;
                }

                var startDate = application.StartDate;
                var endDate = startDate.AddMonths(12);  // Example duration

                _context.TenantPlanAssignments.Add(new DealerPlanAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CategoryId = application.CategoryId,
                    DealerId = dto.DealerId,
                    PlanType = dto.PlanType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = dto.Status,
                    SlotsUsed = dto.SlotsUsed,
                    MaxSlots = dto.MaxSlots
                });

                // Auto activate dealer application
                application.Status = "Active";

                assignedDealersCount++;
            }

            await _context.SaveChangesAsync();

            bool success = errors.Count == 0;
            return (success, errors);
        }




        public async Task<bool> UpdateAssignmentAsync(Guid id, DealerPlanAssignmentDto dto)
        {
            var existing = await _context.TenantPlanAssignments.FindAsync(id);
            if (existing == null) return false;

            existing.CategoryId = dto.CategoryId;
            existing.DealerId = dto.DealerId;
            existing.PlanType = dto.PlanType;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.Status = dto.Status;
            existing.SlotsUsed = dto.SlotsUsed;
            existing.MaxSlots = dto.MaxSlots;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAssignmentAsync(Guid id)
        {
            var existing = await _context.TenantPlanAssignments.FindAsync(id);
            if (existing == null) return false;

            _context.TenantPlanAssignments.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
