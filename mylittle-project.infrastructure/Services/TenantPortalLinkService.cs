using mylittle_project.Application.DTOs;
using mylittle_project.Application.DTOs.Common;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class TenantPortalLinkService : ITenantPortalLinkService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TenantPortalLinkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddLinkAsync(TenantPortalLinkDto dto)
        {
            var entity = new TenantPortalLink
            {
                Id = Guid.NewGuid(),
                SourceTenantId = dto.SourceTenantId,
                TargetTenantId = dto.TargetTenantId,
                LinkType = dto.LinkType,
                LinkedSince = dto.LinkedSince ?? DateTime.UtcNow
            };

            await _unitOfWork.TenantPortalLinks.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task AddLinksBatchAsync(TenantPortalLinkBatchDto dto)
        {
            var links = dto.TargetTenantIds.Select(targetId => new TenantPortalLink
            {
                Id = Guid.NewGuid(),
                SourceTenantId = dto.SourceTenantId,
                TargetTenantId = targetId,
                LinkType = dto.LinkType,
                LinkedSince = DateTime.UtcNow
            });

            await _unitOfWork.TenantPortalLinks.AddRangeAsync(links);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<TenantPortalLinkViewDto>> GetAllLinkedPortalsAsync()
        {
            var links = await _unitOfWork.TenantPortalLinks.GetAllAsync();
            return links.Select(x => new TenantPortalLinkViewDto
            {
                SourceTenantId = x.SourceTenantId,
                TargetTenantId = x.TargetTenantId,
                LinkType = x.LinkType,
                LinkedSince = x.LinkedSince
            });
        }

        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
        {
            var tenants = await _unitOfWork.Tenants.GetAllAsync();
            return tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                TenantName = t.TenantName
            });
        }

        public async Task<PaginatedResult<TenantPortalLinkDto>> GetPaginatedLinkedPortalsAsync(int page, int pageSize)
        {
            return await _unitOfWork.TenantPortalLinks.GetFilteredAsync(
                filter: null,
                selector: x => new TenantPortalLinkDto
                {
                    Id = x.Id,
                    SourceTenantId = x.SourceTenantId,
                    TargetTenantId = x.TargetTenantId,
                    LinkType = x.LinkType,
                    LinkedSince = x.LinkedSince
                },
                page: page,
                pageSize: pageSize,
                sortBy: "LinkedSince",
                sortDir: "desc"
            );
        }
    }
}
