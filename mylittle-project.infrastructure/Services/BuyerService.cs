using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.DTOs.Common;
using mylittle_project.Application.Interfaces;
using mylittle_project.Application.Interfaces.Repositories;
using mylittle_project.Domain.Entities;
using System.Linq.Expressions;

namespace mylittle_project.Infrastructure.Services
{
    public class BuyerService : IBuyerService
    {
        private readonly IGenericRepository<Buyer> _repository;
        private readonly IHttpContextAccessor _httpContext;

        public BuyerService(
            IGenericRepository<Buyer> repository,
            IHttpContextAccessor httpContext)
        {
            _repository = repository;
            _httpContext = httpContext;
        }

        private Guid GetTenantId()
        {
            var tenantIdHeader = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantIdHeader == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantIdHeader);
        }

        public async Task<Guid> CreateBuyerAsync(BuyerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Buyer name is required.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                throw new ArgumentException("A valid email is required.");

            var buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Country = dto.Country,
                TenantId = dto.TenantId,
                BusinessId = dto.BusinessId,
                IsActive = true,
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(buyer);
            await _repository.SaveAsync();
            return buyer.Id;
        }

        public async Task<List<BuyerListDto>> GetAllBuyersAsync()
        {
            return await _repository.Find(b => !b.IsDeleted)
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
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<BuyerListDto>> GetAllBuyersPaginatedAsync(int page, int pageSize)
        {
            Expression<Func<Buyer, bool>> predicate = b => !b.IsDeleted;

            Expression<Func<Buyer, BuyerListDto>> selector = b => new BuyerListDto
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
            };

            return await _repository.GetFilteredAsync(predicate, selector, page, pageSize, "CreatedAt", "desc");
        }

        public async Task<List<BuyerListDto>> GetBuyersByBusinessAsync(Guid businessId)
        {
            return await _repository.Find(b => b.BusinessId == businessId && !b.IsDeleted)
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
            var buyer = await _repository.GetByIdAsync(buyerId);
            if (buyer == null || buyer.IsDeleted) return false;

            buyer.IsDeleted = true;
            buyer.DeletedAt = DateTime.UtcNow;

            _repository.Update(buyer);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<BuyerSummaryDto?> GetBuyerProfileAsync(Guid buyerId)
        {
            var buyer = await _repository.Find(b => b.Id == buyerId)
                .Include(b => b.Orders)
                .Include(b => b.ActivityLogs)
                .FirstOrDefaultAsync();

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
            var buyer = await _repository.GetByIdAsync(buyerId);
            if (buyer == null || buyer.IsDeleted) return false;

            buyer.Name = dto.Name;
            buyer.Phone = dto.Phone;
            buyer.Country = dto.Country;
            buyer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(buyer);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<BuyerListDto?> GetBuyerByIdAsync(Guid id)
        {
            var buyer = await _repository.Find(b => b.Id == id && !b.IsDeleted)
                .Include(b => b.Orders)
                .FirstOrDefaultAsync();

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
    }
}
