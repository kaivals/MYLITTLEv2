using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace mylittle_project.infrastructure.Services
{
    public class TenantSubscriptionService : ITenantSubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly IGlobalSubscriptionService _globalService;

        public TenantSubscriptionService(AppDbContext context, IGlobalSubscriptionService globalService)
        {
            _context = context;
            _globalService = globalService;
        }

        public async Task<List<TenantSubscription>> GetByTenantAsync(Guid tenantId) =>
            await _context.TenantSubscriptions.Where(t => t.TenantId == tenantId).ToListAsync();

        public async Task UpdateOrAddPlansAsync(Guid tenantId, List<TenantSubscriptionDto> newPlans)
        {
            var existingPlans = await _context.TenantSubscriptions
                                              .Where(p => p.TenantId == tenantId)
                                              .ToListAsync();

            foreach (var dto in newPlans)
            {
                var existing = existingPlans
                    .FirstOrDefault(p => p.PlanName.ToLower() == dto.PlanName.ToLower());

                var global = await _globalService.GetByNameAsync(dto.PlanName);
                if (global == null)
                    throw new Exception($"Global plan '{dto.PlanName}' not found.");

                if (existing != null)
                {
                    // Update existing plan
                    existing.PlanCost = dto.PlanCost;
                    existing.NumberOfAds = dto.NumberOfAds;
                    existing.MaxEssentialMembers = dto.MaxEssentialMembers;
                    existing.MaxPremiumMembers = dto.MaxPremiumMembers;
                    existing.MaxEliteMembers = dto.MaxEliteMembers;
                    existing.IsTrial = dto.IsTrial;
                    existing.IsActive = dto.IsActive;
                }
                else
                {
                    // Add new plan
                    _context.TenantSubscriptions.Add(new TenantSubscription
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        GlobalPlanId = global.Id,
                        PlanName = dto.PlanName,
                        PlanCost = dto.PlanCost,
                        NumberOfAds = dto.NumberOfAds,
                        MaxEssentialMembers = dto.MaxEssentialMembers,
                        MaxPremiumMembers = dto.MaxPremiumMembers,
                        MaxEliteMembers = dto.MaxEliteMembers,
                        IsTrial = dto.IsTrial,
                        IsActive = dto.IsActive
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task AddCustomPlansToTenantAsync(Guid tenantId, List<TenantSubscriptionDto> plans)
        {
            foreach (var dto in plans)
            {
                var global = await _globalService.GetByNameAsync(dto.PlanName);
                if (global == null) throw new Exception($"Global plan '{dto.PlanName}' not found.");

                _context.TenantSubscriptions.Add(new TenantSubscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    GlobalPlanId = global.Id,
                    PlanName = dto.PlanName,
                    PlanCost = dto.PlanCost,
                    NumberOfAds = dto.NumberOfAds,
                    MaxEssentialMembers = dto.MaxEssentialMembers,
                    MaxPremiumMembers = dto.MaxPremiumMembers,
                    MaxEliteMembers = dto.MaxEliteMembers,
                    IsTrial = dto.IsTrial,
                    IsActive = dto.IsActive
                });
            }
            await _context.SaveChangesAsync();
        }
    }

}
