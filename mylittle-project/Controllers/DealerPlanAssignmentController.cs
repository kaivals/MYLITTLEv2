using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.infrastructure.Services;

[ApiController]
[Route("api/dealer-plan-assignments")]
public class DealerPlanAssignmentController : ControllerBase
{
    private readonly IDealerPlanAssignmentService _service;

    public DealerPlanAssignmentController(IDealerPlanAssignmentService service)
    {
        _service = service;
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> Get(Guid tenantId)
    {
        var plans = await _service.GetByTenantAsync(tenantId);
        return Ok(plans);
    }

    [HttpGet("tenant/{tenantId}/scheduler")]
    public async Task<IActionResult> GetSchedulerAssignments(Guid tenantId)
    {
        var result = await _service.GetSchedulerAssignmentsAsync(tenantId);
        return Ok(result);
    }

    [HttpPost("tenant/{tenantId}")]
    public async Task<IActionResult> Add(Guid tenantId, [FromBody] List<DealerPlanAssignmentDto> assignments)
    {
        var (success, errors) = await _service.AddAssignmentsAsync(tenantId, assignments);


        if (!success)
        {
            return BadRequest(new
            {
                message = "Some assignments failed.",
                errors
            });
        }

        return Ok(new { message = "Assignments added successfully." });
    }


    [HttpPut("{assignmentId}")]
    public async Task<IActionResult> Update(Guid assignmentId, [FromBody] DealerPlanAssignmentDto dto)
    {
        var result = await _service.UpdateAssignmentAsync(assignmentId, dto);
        if (!result)
            return NotFound(new { Message = "Assignment not found." });

        return Ok(new { Message = "Updated successfully." });
    }

    [HttpDelete("{assignmentId}")]
    public async Task<IActionResult> Delete(Guid assignmentId)
    {
        var result = await _service.DeleteAssignmentAsync(assignmentId);
        if (!result)
            return NotFound(new { Message = "Assignment not found." });

        return Ok(new { Message = "Deleted successfully." });
    }
}
