using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class VirtualNumberService : IVirtualNumberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VirtualNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> AssignVirtualNumberAsync(Guid DealerId)
        {
            var exists = await _unitOfWork.VirtualNumberAssignments
                .Find(v => v.DealerId == DealerId)
                .AnyAsync();

            if (exists)
                throw new InvalidOperationException("Virtual number already assigned to this business.");

            // Generate Virtual Number Dynamically
            var number = "+91-" + Math.Abs((DealerId.ToString() + DateTime.UtcNow.Ticks).GetHashCode())
                .ToString().PadLeft(10, '0').Substring(0, 10);

            var assignment = new VirtualNumberAssignment
            {
                Id = Guid.NewGuid(),
                DealerId = DealerId,
                VirtualNumber = number
            };

            await _unitOfWork.VirtualNumberAssignments.AddAsync(assignment);
            await _unitOfWork.SaveAsync();

            return number;
        }
        public async Task<bool> DeleteVirtualNumberAsync(Guid dealerId)
        {
            var entity = await _unitOfWork.VirtualNumberAssignments
                .Find(v => v.DealerId == dealerId)
                .FirstOrDefaultAsync();

            if (entity == null || entity.IsDeleted)
                return false;

            entity.IsDeleted = true;
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> RestoreVirtualNumberAsync(Guid dealerId)
        {
            var entity = await _unitOfWork.VirtualNumberAssignments
                .Find(v => v.DealerId == dealerId)
                .FirstOrDefaultAsync();

            if (entity == null || !entity.IsDeleted)
                return false;

            entity.IsDeleted = false;
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<string> GetAssignedNumberAsync(Guid DealerId)
        {
            var assignment = await _unitOfWork.VirtualNumberAssignments
                .Find(v => v.DealerId == DealerId)
                .FirstOrDefaultAsync();

            return assignment?.VirtualNumber;
        }
    }
}
