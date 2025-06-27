using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/filters")]
    public class FiltersController : ControllerBase
    {
        private readonly IFilterService _filterService;

        public FiltersController(IFilterService filterService)
        {
            _filterService = filterService;
        }

        // GET: /api/filters
        [HttpGet]
        public async Task<ActionResult<List<FilterDto>>> GetAll()
        {
            var filters = await _filterService.GetAllAsync();
            return Ok(filters);
        }

        // GET: /api/filters/paginated?page=1&pageSize=10
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResult<FilterDto>>> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _filterService.GetPaginatedAsync(page, pageSize);
            return Ok(result);
        }

        // GET: /api/filters/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FilterDto>> GetById(Guid id)
        {
            var filter = await _filterService.GetByIdAsync(id);
            return filter == null ? NotFound() : Ok(filter);
        }

        // POST: /api/filters
        [HttpPost]
        public async Task<ActionResult<FilterDto>> Create([FromBody] CreateFilterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _filterService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: /api/filters/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FilterDto>> Update(Guid id, [FromBody] CreateFilterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _filterService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        // DELETE: /api/filters/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _filterService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
