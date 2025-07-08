using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
{
    public class DealerService : IDealerService
    {
        private readonly AppDbContext _context;

        public DealerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateBusinessInfoAsync(DealerDto dto)
        {
            var dealer = new Dealer
            {
                TenantId = dto.TenantId,
                DealerName = dto.DealerName,
                BusinessName = dto.BusinessName,
                BusinessNumber = dto.BusinessNumber,
                BusinessEmail = dto.BusinessEmail,
                BusinessAddress = dto.BusinessAddress,
                ContactEmail = dto.ContactEmail,
                PhoneNumber = dto.PhoneNumber,
                Website = dto.Website,
                TaxId = dto.TaxIdOrGstNumber,
                Country = dto.Country,
                State = dto.State,
                City = dto.City,
                Timezone = dto.Timezone
            };

            _context.Dealers.Add(dealer);
            await _context.SaveChangesAsync();

            // Virtual Number Assignment
            var virtualNumber = "VN" + DateTime.UtcNow.Ticks.ToString().Substring(5, 10);
            var virtualAssignment = new VirtualNumberAssignment
            {
                DealerId = dealer.Id,
                VirtualNumber = virtualNumber
            };

            _context.VirtualNumberAssignments.Add(virtualAssignment);
            await _context.SaveChangesAsync();

            return dealer.Id;
        }

        // ✅ Get All Dealers (For Tenant Owner/Admin)
        public async Task<List<object>> GetAllDealersAsync()
        {
            return await GetDealersWithDetailsAsync();
        }

        // ✅ Get Dealers By Tenant
        public async Task<List<object>> GetDealersByTenantAsync(Guid tenantId)
        {
            return await GetDealersWithDetailsAsync(tenantId);
        }

        // ✅ Shared Private Method (NO UserDealers Navigation Anymore)
        private async Task<List<object>> GetDealersWithDetailsAsync(Guid? tenantId = null)
        {
            var query = _context.Dealers
                .Include(d => d.Tenant)
                .Include(d => d.VirtualNumberAssignment)
                .Include(d => d.KycDocumentUploads)
                .Include(d => d.DealerSubscriptions)
                .AsQueryable();

            if (tenantId.HasValue)
            {
                query = query.Where(d => d.TenantId == tenantId.Value);
            }

            var dealers = await query.ToListAsync();

            return dealers.Select(d => new
            {
                DealerId = d.Id,
                d.TenantId,
                d.DealerName,
                d.BusinessName,
                d.BusinessNumber,
                d.BusinessEmail,
                d.BusinessAddress,
                d.ContactEmail,
                d.PhoneNumber,
                d.Website,
                d.TaxId,
                d.Country,
                d.State,
                d.City,
                d.Timezone,
                PortalName = d.Tenant?.TenantName,
                VirtualNumber = d.VirtualNumberAssignment?.VirtualNumber,
                KycDocuments = d.KycDocumentUploads?.Select(k => new
                {
                    k.Id,
                    k.DocType,
                    k.FileUrl,
                    k.UploadedAt
                }),
                DealerSubscriptions = d.DealerSubscriptions?.Select(s => new
                {
                    s.Id,
                    s.CategoryId,
                    s.PlanType,
                    s.StartDate,
                    s.Status,
                    s.IsQueued
                })
            }).ToList<object>();
        }
    }
}
