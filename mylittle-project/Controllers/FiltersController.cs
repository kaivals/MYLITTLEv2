using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;

namespace mylittle_project.API.Controllers
{
    // Defines this class as an API controller with routing like /api/filters
    [ApiController]
    [Route("api/filters")]
    public class FiltersController : ControllerBase
    {
        private readonly IFilterService _filterService;

        // Constructor injection for the IFilterService (business logic layer)
        public FiltersController(IFilterService filterService)
        {
            _filterService = filterService;
        }

        // ───────────────────── GET: /api/filters ─────────────────────
        // Returns a list of all filters
        [HttpGet]
        public async Task<ActionResult<List<FilterDto>>> GetAll()
        {
            var filters = await _filterService.GetAllAsync(); // Fetch from DB via service
            return Ok(filters); // 200 OK response
        }

        // ───────────────────── GET: /api/filters/{id} ─────────────────────
        // Returns a single filter by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<FilterDto>> GetById(Guid id)
        {
            var filter = await _filterService.GetByIdAsync(id);
            if (filter == null)
                return NotFound(); // 404 if not found

            return Ok(filter); // 200 OK with filter data
        }

        // ───────────────────── POST: /api/filters ─────────────────────
        // Creates a new filter with a list of values
        [HttpPost]
        public async Task<ActionResult<FilterDto>> Create([FromBody] CreateFilterDto dto)
        {
            var created = await _filterService.CreateAsync(dto); // Call service to add filter
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // 201 Created
        }

        // ───────────────────── PUT: /api/filters/{id} ─────────────────────
        // Updates an existing filter's name and values
        [HttpPut("{id}")]
        public async Task<ActionResult<FilterDto>> Update(Guid id, [FromBody] CreateFilterDto dto)
        {
            var updated = await _filterService.UpdateAsync(id, dto); // Call service to update
            if (updated == null)
                return NotFound(); // 404 if ID not found

            return Ok(updated); // 200 OK with updated data
        }

        // ───────────────────── DELETE: /api/filters/{id} ─────────────────────
        // Deletes a filter by its ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _filterService.DeleteAsync(id); // Call service to delete
            if (!deleted)
                return NotFound(); // 404 if not found

            return NoContent(); // 204 No Content (successful delete)
        }
    }
}
