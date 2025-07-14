using mylittle_project.Application.DTOs;
using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IDealerPlanAssignmentService
    {
        Task<List<DealerPlanAssignment>> GetByTenantAsync(Guid tenantId);
        Task<(bool Success, List<string> Errors)> AddAssignmentsAsync(Guid tenantId, List<DealerPlanAssignmentDto> assignments);

        Task<bool> UpdateAssignmentAsync(Guid assignmentId, DealerPlanAssignmentDto dto);
        Task<bool> SoftDeleteAssignmentAsync(Guid id);

        Task<List<SchedulerAssignmentDto>> GetSchedulerAssignmentsAsync(Guid tenantId);

    }
}
