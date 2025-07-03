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
        private readonly IGenericRepository<ActivityLogBuyer> _activityRepository;
        private readonly IGenericRepository<Buyer> _repository;
        private readonly IHttpContextAccessor _httpContext;

        public BuyerService(
            IGenericRepository<Buyer> repository,
            IGenericRepository<ActivityLogBuyer> activityRepository,
            IHttpContextAccessor httpContext)
        {
            _repository = repository;
            _activityRepository = activityRepository;
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
                DealerId = dto.DealerId,
                IsActive = true,
                Status = "Active"
            };


            var log = new ActivityLogBuyer
            {
                Id = Guid.NewGuid(),
                BuyerId = buyer.Id,
                TenantId = buyer.TenantId,
                Activity = "Buyer Created",
                Description = $"Buyer '{buyer.Name}' was created.",
                Timestamp = DateTime.UtcNow
            };

   
            await _repository.AddAsync(buyer);
            await _repository.SaveAsync();
            return buyer.Id;
        }

        public async Task<List<BuyerListDto>> GetAllBuyersAsync()
        {
            return await _repository.Find(b => !b.IsDeleted)
                .Include(b => b.Orders)
                .Include(b => b.Tenant) // ✅ Add this
                .Select(b => new BuyerListDto
                {
                    Id = b.Id,
                    BuyerName = b.Name,
                    Email = b.Email,
                    PhoneNumber = b.Phone,
                    TotalOrders = b.Orders.Count,
                    TenantId = b.TenantId,
                    DealerId = b.DealerId,
                    IsActive = b.IsActive,
                    Status = b.Status,
                    TenantName = b.Tenant != null ? b.Tenant.TenantName : ""
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
                DealerId = b.DealerId,
                IsActive = b.IsActive,
                Status = b.Status,
                TenantName = b.Tenant != null ? b.Tenant.TenantName : ""
            };

            return await _repository.GetAll()
                .Include(b => b.Tenant)
                .Include(b => b.Orders)
                .Where(predicate)
                .Select(selector)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ContinueWith(task => new PaginatedResult<BuyerListDto>
                {
                    Items = task.Result,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = _repository.Find(predicate).Count()
                });
        }

        public async Task<List<BuyerListDto>> GetBuyersByBusinessAsync(Guid DealerId)
        {
            return await _repository.Find(b => b.DealerId == DealerId && !b.IsDeleted)
                .Include(b => b.Orders)
                .Include(b => b.Tenant)
                .Select(b => new BuyerListDto
                {
                    Id = b.Id,
                    BuyerName = b.Name,
                    Email = b.Email,
                    PhoneNumber = b.Phone,
                    TotalOrders = b.Orders.Count,
                    TenantId = b.TenantId,
                    DealerId = b.DealerId,
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
            var log = new ActivityLogBuyer
            {
                Id = Guid.NewGuid(),
                BuyerId = buyer.Id,
                TenantId = buyer.TenantId,
                Activity = "Buyer Soft Deleted",
                Description = $"Buyer '{buyer.Name}' was soft-deleted.",
                Timestamp = DateTime.UtcNow
            };
            await _activityRepository.AddAsync(log);
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
                DealerId = buyer.DealerId,
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
            var log = new ActivityLogBuyer
            {
                Id = Guid.NewGuid(),
                BuyerId = buyer.Id,
                TenantId = buyer.TenantId,
                Activity = "Buyer Updated",
                Description = $"Buyer '{buyer.Name}' was updated.",
                Timestamp = DateTime.UtcNow
            };
            await _activityRepository.AddAsync(log);
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
                DealerId = buyer.DealerId,
                IsActive = buyer.IsActive,
                Status = buyer.Status
            };
        }
    }
}
