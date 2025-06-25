using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class VirtualNumberService : IVirtualNumberService
{
    private readonly AppDbContext _context;

    public VirtualNumberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> AssignVirtualNumberAsync(Guid businessId)
    {
        if (await _context.VirtualNumberAssignments.AnyAsync(v => v.BusinessId == businessId))
            throw new InvalidOperationException("Virtual number already assigned to this business.");

        // Generate a virtual number dynamically (e.g., based on timestamp and business ID hash)
        var number = "+91-" + Math.Abs((businessId.ToString() + DateTime.UtcNow.Ticks).GetHashCode()).ToString().PadLeft(10, '0').Substring(0, 10);

        var assignment = new VirtualNumberAssignment
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            VirtualNumber = number
        };

        _context.VirtualNumberAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return number;
    }

    public async Task<string> GetAssignedNumberAsync(Guid businessId)
    {
        var assignment = await _context.VirtualNumberAssignments
            .FirstOrDefaultAsync(v => v.BusinessId == businessId);

        return assignment?.VirtualNumber;
    }
}
