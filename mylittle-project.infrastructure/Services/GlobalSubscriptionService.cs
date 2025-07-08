using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
{
    public class GlobalSubscriptionService : IGlobalSubscriptionService
    {
        private readonly AppDbContext _context;

        public GlobalSubscriptionService(AppDbContext context) => _context = context;

        public async Task<List<GlobalSubscription>> GetAllAsync() =>
            await _context.GlobalSubscriptions.ToListAsync();

        public async Task<GlobalSubscription?> GetByNameAsync(string name) =>
            await _context.GlobalSubscriptions
                          .FirstOrDefaultAsync(p => p.PlanName.ToLower() == name.ToLower());

        public async Task<GlobalSubscription> CreateAsync(GlobalSubscriptionDto dto)
        {
            // Validate uniqueness globally
            var exists = await _context.GlobalSubscriptions
                .AnyAsync(p => p.PlanName.ToLower() == dto.PlanName.Trim().ToLower());

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

            _context.GlobalSubscriptions.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }
    }
}
