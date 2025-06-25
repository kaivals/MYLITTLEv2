using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;

public class BuyerService : IBuyerService
{
    private readonly AppDbContext _context;

    public BuyerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateBuyerAsync(BuyerCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Buyer name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            throw new ArgumentException("A valid email is required.");

        var buyer = new Buyer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Country = dto.Country,
            TenantId = dto.TenantId,
            BusinessId = dto.BusinessId,
            IsActive = true,
            Status = "Active"
        };

        _context.Buyers.Add(buyer);
        await _context.SaveChangesAsync(); // Save first to get buyer.Id

        await LogActivityAsync(buyer.Id, buyer.TenantId, "Create Buyer", $"Buyer '{buyer.Name}' created.");
        await _context.SaveChangesAsync(); // Save activity

        return buyer.Id;
    }

    public async Task<List<BuyerListDto>> GetAllBuyersAsync()
    {
        return await _context.Buyers
            .Where(b => !b.IsDeleted)
            .Include(b => b.Orders)
            .Select(b => new BuyerListDto
            {
                Id = b.Id,
                BuyerName = b.Name,
                Email = b.Email,
                PhoneNumber = b.Phone,
                TotalOrders = b.Orders.Count,
                TenantId = b.TenantId,
                BusinessId = b.BusinessId,
                IsActive = b.IsActive,
                Status = b.Status
            }).ToListAsync();
    }

    public async Task<List<BuyerListDto>> GetBuyersByBusinessAsync(Guid businessId)
    {
        return await _context.Buyers
            .Where(b => b.BusinessId == businessId && !b.IsDeleted)
            .Include(b => b.Orders)
            .Select(b => new BuyerListDto
            {
                Id = b.Id,
                BuyerName = b.Name,
                Email = b.Email,
                PhoneNumber = b.Phone,
                TotalOrders = b.Orders.Count,
                TenantId = b.TenantId,
                BusinessId = b.BusinessId,
                IsActive = b.IsActive,
                Status = b.Status
            }).ToListAsync();
    }

    public async Task<bool> SoftDeleteBuyerAsync(Guid buyerId)
    {
        var buyer = await _context.Buyers.FindAsync(buyerId);
        if (buyer == null || buyer.IsDeleted) return false;

        buyer.IsDeleted = true;
        buyer.DeletedAt = DateTime.UtcNow;

        _context.Buyers.Update(buyer);
        await LogActivityAsync(buyer.Id, buyer.TenantId, "Soft Delete Buyer", $"Buyer '{buyer.Name}' was soft deleted.");
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<BuyerSummaryDto?> GetBuyerProfileAsync(Guid buyerId)
    {
        var buyer = await _context.Buyers
            .Include(b => b.Orders)
            .Include(b => b.ActivityLogs)
            .FirstOrDefaultAsync(b => b.Id == buyerId);

        if (buyer == null || buyer.IsDeleted) return null;

        return new BuyerSummaryDto
        {
            Id = buyer.Id,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            Country = buyer.Country,
            RegistrationDate = buyer.RegistrationDate,
            LastLogin = buyer.LastLogin,
            Status = buyer.Status,
            IsActive = buyer.IsActive,
            BusinessId = buyer.BusinessId,
            TenantId = buyer.TenantId,
            TotalOrders = buyer.Orders?.Count ?? 0,
            TotalActivities = buyer.ActivityLogs?.Count ?? 0
        };
    }

    public async Task<bool> UpdateBuyerAsync(Guid buyerId, BuyerUpdateDto dto)
    {
        var buyer = await _context.Buyers.FindAsync(buyerId);
        if (buyer == null || buyer.IsDeleted) return false;

        buyer.Name = dto.Name;
        buyer.Phone = dto.Phone;
        buyer.Country = dto.Country;

        _context.Buyers.Update(buyer);
        await LogActivityAsync(buyer.Id, buyer.TenantId, "Update Buyer", $"Buyer '{buyer.Name}' updated.");
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<BuyerListDto?> GetBuyerByIdAsync(Guid id)
    {
        var buyer = await _context.Buyers
            .Include(b => b.Orders)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        if (buyer == null)
            return null;

        return new BuyerListDto
        {
            Id = buyer.Id,
            BuyerName = buyer.Name,
            Email = buyer.Email,
            PhoneNumber = buyer.Phone,
            TotalOrders = buyer.Orders.Count,
            TenantId = buyer.TenantId,
            BusinessId = buyer.BusinessId,
            IsActive = buyer.IsActive,
            Status = buyer.Status
        };
    }

    // ✅ Centralized activity logger
    private async Task LogActivityAsync(Guid buyerId, Guid tenantId, string action, string description)
    {
        var log = new ActivityLogBuyer
        {
            TenantId = tenantId,
            BuyerId = buyerId,
            Activity = action,
            Description = description,
            Timestamp = DateTime.UtcNow
        };

        await _context.ActivityLogs.AddAsync(log);
    }
}
