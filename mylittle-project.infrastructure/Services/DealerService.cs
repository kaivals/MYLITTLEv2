using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.infrastructure.Services
{
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DealerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Dealers.Add(dealer);
                await _unitOfWork.SaveAsync();

                var virtualNumber = "VN" + DateTime.UtcNow.Ticks.ToString().Substring(5, 10);
                var virtualAssignment = new VirtualNumberAssignment
                {
                    DealerId = dealer.Id,
                    VirtualNumber = virtualNumber
                };

                _unitOfWork.VirtualNumberAssignments.Add(virtualAssignment);
                await _unitOfWork.SaveAsync();

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return dealer.Id;
        }

        public async Task<List<object>> GetAllDealersAsync()
        {
            return await GetDealersWithDetailsAsync();
        }

        public async Task<List<object>> GetDealersByTenantAsync(Guid tenantId)
        {
            return await GetDealersWithDetailsAsync(tenantId);
        }

        private async Task<List<object>> GetDealersWithDetailsAsync(Guid? tenantId = null)
        {
            var query = _unitOfWork.Dealers
                .Find(d => true)
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
