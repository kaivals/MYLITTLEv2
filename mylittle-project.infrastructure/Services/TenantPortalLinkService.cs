using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace mylittle_project.infrastructure.Services
{
    public class TenantPortalLinkService : ITenantPortalLinkService
    {
        private readonly AppDbContext _context;

        public TenantPortalLinkService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLinkAsync(TenentPortalLinkDto dto)
        {
            var link = new TenentPortalLink
            {
                SourceTenantId = dto.SourceTenantId,
                TargetTenantId = dto.TargetTenantId,
                LinkType = dto.LinkType,
                LinkedSince = dto.LinkedSince
            };

            _context.TenentPortalLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        public async Task AddLinksBatchAsync(TenentPortalLinkBatchDto dto)
        {
            var links = dto.TargetTenantIds.Select(targetId => new TenentPortalLink
            {
                SourceTenantId = dto.SourceTenantId,
                TargetTenantId = targetId,
                LinkType = dto.LinkType
            }).ToList();

            _context.TenentPortalLinks.AddRange(links);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TenentPortalLinkViewDto>> GetAllLinkedPortalsAsync()
        {
            var links = await _context.TenentPortalLinks
                .Include(l => l.SourceTenant)
                .Include(l => l.TargetTenant)
                .ToListAsync();

            return links.Select(link => new TenentPortalLinkViewDto
            {
                SourceTenantId = link.SourceTenantId,
                SourceTenantName = link.SourceTenant?.TenantName,
                TargetTenantId = link.TargetTenantId,
                TargetTenantName = link.TargetTenant?.TenantName,
                LinkType = link.LinkType,
                LinkedSince = link.LinkedSince
            });
        }

        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
        {
            var tenants = await _context.Tenants.ToListAsync();

            return tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                TenantName = t.TenantName,
                TenantNickname = t.TenantNickname,
                Subdomain = t.Subdomain,
                IndustryType = t.IndustryType,
                Status = t.Status,
                Description = t.Description,
                IsActive = t.IsActive,
                LastAccessed = t.LastAccessed
            });
        }
    }
}
