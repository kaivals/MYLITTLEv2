using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class TenantSubscriptionService : ITenantSubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGlobalSubscriptionService _globalService;

        public TenantSubscriptionService(IUnitOfWork unitOfWork, IGlobalSubscriptionService globalService)
        {
            _unitOfWork = unitOfWork;
            _globalService = globalService;
        }

        public async Task<List<TenantSubscription>> GetByTenantAsync(Guid tenantId)
        {
            return await _unitOfWork.TenantSubscriptions
                .Find(t => t.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task UpdateOrAddPlansAsync(Guid tenantId, List<TenantSubscriptionDto> newPlans)
        {
            var existingPlans = await _unitOfWork.TenantSubscriptions
                .Find(p => p.TenantId == tenantId)
                .ToListAsync();

            var globalPlans = await _globalService.GetAllAsync();

            foreach (var global in globalPlans)
            {
                var dto = newPlans.FirstOrDefault(p =>
                    p.PlanName.Equals(global.PlanName, StringComparison.OrdinalIgnoreCase));
                var existing = existingPlans.FirstOrDefault(p =>
                    p.PlanName.Equals(global.PlanName, StringComparison.OrdinalIgnoreCase));

                if (dto != null)
                {
                    if (existing != null)
                    {
                        existing.PlanCost = dto.PlanCost;
                        existing.NumberOfAds = dto.NumberOfAds;
                        existing.MaxMembers = dto.MaxMembers;
                        existing.IsTrial = dto.IsTrial;
                        existing.IsActive = dto.IsActive;
                        _unitOfWork.TenantSubscriptions.Update(existing);
                    }
                    else
                    {
                        await _unitOfWork.TenantSubscriptions.AddAsync(new TenantSubscription
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            GlobalPlanId = global.Id,
                            PlanName = dto.PlanName,
                            PlanCost = dto.PlanCost,
                            NumberOfAds = dto.NumberOfAds,
                            MaxMembers = dto.MaxMembers,
                            IsTrial = dto.IsTrial,
                            IsActive = dto.IsActive
                        });
                    }
                }
                else if (existing == null)
                {
                    await _unitOfWork.TenantSubscriptions.AddAsync(new TenantSubscription
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        GlobalPlanId = global.Id,
                        PlanName = global.PlanName,
                        PlanCost = global.PlanCost,
                        NumberOfAds = global.NumberOfAds,
                        MaxMembers = global.MaxMembers,
                        IsTrial = global.IsTrial,
                        IsActive = global.IsActive
                    });
                }
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task AddCustomPlansToTenantAsync(Guid tenantId, List<TenantSubscriptionDto> plans)
        {
            var duplicateNames = plans.GroupBy(p => p.PlanName.Trim().ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Any())
                throw new Exception($"Duplicate plan names in request: {string.Join(", ", duplicateNames)}");

            var existingTenantPlanNames = await _unitOfWork.TenantSubscriptions
                .Find(t => t.TenantId == tenantId)
                .Select(t => t.PlanName.ToLower())
                .ToListAsync();

            foreach (var plan in plans)
            {
                if (existingTenantPlanNames.Contains(plan.PlanName.Trim().ToLower()))
                    throw new Exception($"Plan name '{plan.PlanName}' already exists for this tenant.");
            }

            foreach (var dto in plans)
            {
                var global = await _globalService.GetByNameAsync(dto.PlanName);
                if (global == null)
                    throw new Exception($"Global plan '{dto.PlanName}' not found.");

                await _unitOfWork.TenantSubscriptions.AddAsync(new TenantSubscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    GlobalPlanId = global.Id,
                    PlanName = dto.PlanName,
                    PlanCost = dto.PlanCost,
                    NumberOfAds = dto.NumberOfAds,
                    MaxMembers = dto.MaxMembers,
                    IsTrial = dto.IsTrial,
                    IsActive = dto.IsActive
                });
            }

            await _unitOfWork.SaveAsync();
        }
    }
}
