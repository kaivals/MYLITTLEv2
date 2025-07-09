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
    public class GlobalSubscriptionService : IGlobalSubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GlobalSubscriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GlobalSubscription>> GetAllAsync()
        {
            return (await _unitOfWork.GlobalSubscriptions.GetAllAsync()).ToList();
        }

        public async Task<GlobalSubscription?> GetByNameAsync(string name)
        {
            return await _unitOfWork.GlobalSubscriptions
                .Find(p => p.PlanName.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<GlobalSubscription> CreateAsync(GlobalSubscriptionDto dto)
        {
            var exists = await _unitOfWork.GlobalSubscriptions
                .Find(p => p.PlanName.ToLower() == dto.PlanName.Trim().ToLower())
                .AnyAsync();

            if (exists)
                throw new Exception($"Global plan with name '{dto.PlanName}' already exists.");

            var plan = new GlobalSubscription
            {
                Id = Guid.NewGuid(),
                PlanName = dto.PlanName,
                Description = dto.Description,
                PlanCost = dto.PlanCost,
                NumberOfAds = dto.NumberOfAds,
                MaxMembers = dto.MaxMembers,
                IsTrial = dto.IsTrial,
                IsActive = dto.IsActive
            };

            await _unitOfWork.GlobalSubscriptions.AddAsync(plan);
            await _unitOfWork.SaveAsync();
            return plan;
        }
    }
}
