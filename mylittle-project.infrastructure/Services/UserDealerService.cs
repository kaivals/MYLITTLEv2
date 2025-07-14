using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserDealerService : IUserDealerService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserDealerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> AddUserAsync(UserDealerDto dto)
    {
        var business = await _unitOfWork.Dealers.GetAll()
            .FirstOrDefaultAsync(b => b.Id == dto.DealerId);

        if (business == null)
            throw new Exception("Invalid DealerId provided.");

        var dealerPortalId = business.TenantId;

        var assignments = new List<PortalAssignment>();

        foreach (var pa in dto.PortalAssignments)
        {
            // Validate Portal
            var assignedPortal = await _unitOfWork.Tenants.GetAll()
                .FirstOrDefaultAsync(t => t.TenantName == pa.PortalName);
            if (assignedPortal == null)
                throw new Exception($"Portal '{pa.PortalName}' does not exist.");

            // Validate Category
            var categoryExists = await _unitOfWork.Categories.GetAll()
                .AnyAsync(c => c.Name == pa.Category);
            if (!categoryExists)
                throw new Exception($"Category '{pa.Category}' does not exist.");

            // Check Link between Dealer's Portal and Assigned Portal
            var isLinked = await _unitOfWork.TenantPortalLinks.GetAll()
                .AnyAsync(link =>
                    (link.SourceTenantId == dealerPortalId && link.TargetTenantId == assignedPortal.Id) ||
                    (link.SourceTenantId == assignedPortal.Id && link.TargetTenantId == dealerPortalId));

            if (isLinked)
            {
                assignments.Add(new PortalAssignment
                {
                    AssignedPortalTenantId = assignedPortal.Id,
                    Category = pa.Category
                });
            }
            else
            {
                throw new Exception($"No portal link exists between dealer portal and '{pa.PortalName}'.");
            }
        }

        var user = new UserDealer
        {
            Username = dto.Username,
            Role = dto.Role,
            IsActive = dto.IsActive,
            DealerId = business.Id,
            PortalAssignments = assignments
        };

        await _unitOfWork.UserDealers.AddAsync(user);
        await _unitOfWork.SaveAsync();
        return user.Id;
    }

    public async Task<List<UserDealerDto>> GetAllUsersAsync()
    {
        return await _unitOfWork.UserDealers.GetAll()
            .Where(u => !u.IsDeleted)
            .Include(u => u.PortalAssignments)
            .ThenInclude(pa => pa.AssignedPortal)
            .Select(u => new UserDealerDto
            {
                DealerId = u.DealerId,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                PortalAssignments = u.PortalAssignments.Select(pa => new PortalAssignmentDto
                {
                    PortalName = pa.AssignedPortal.TenantName,
                    Category = pa.Category
                }).ToList()
            }).ToListAsync();
    }

    public async Task<List<UserDealerDto>> GetUsersByDealerAsync(Guid dealerId)
    {
        return await _unitOfWork.UserDealers.GetAll()
            .Where(u => u.DealerId == dealerId && !u.IsDeleted)
            .Include(u => u.PortalAssignments)
            .ThenInclude(pa => pa.AssignedPortal)
            .Select(u => new UserDealerDto
            {
                DealerId = u.DealerId,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                PortalAssignments = u.PortalAssignments.Select(pa => new PortalAssignmentDto
                {
                    PortalName = pa.AssignedPortal.TenantName,
                    Category = pa.Category
                }).ToList()
            }).ToListAsync();
    }
    public async Task<bool> SoftDeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.UserDealers.GetAll()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.IsDeleted) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        return true;
    }

    // ✅ Restore User
    public async Task<bool> RestoreUserAsync(Guid userId)
    {
        var user = await _unitOfWork.UserDealers.GetAll()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted);

        if (user == null) return false;

        user.IsDeleted = false;
        user.DeletedAt = null;

        await _unitOfWork.SaveAsync();
        return true;
    }
    public async Task<PaginatedResult<UserDealerDto>> GetPaginatedUsersAsync(int page, int pageSize)
    {
        var query = _unitOfWork.UserDealers.GetAll()
            .Where(u => !u.IsDeleted)
            .Include(u => u.PortalAssignments)
            .ThenInclude(pa => pa.AssignedPortal)
            .Select(u => new UserDealerDto
            {
                DealerId = u.DealerId,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                PortalAssignments = u.PortalAssignments.Select(pa => new PortalAssignmentDto
                {
                    PortalName = pa.AssignedPortal.TenantName,
                    Category = pa.Category
                }).ToList()
            });

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<UserDealerDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
