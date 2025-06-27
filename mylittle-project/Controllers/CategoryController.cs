using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;

namespace MyProject.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // ─────────────── GET /api/categories ───────────────
        // Purpose: Fetch paginated list of all categories (for category table)
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var categories = await _categoryService.GetAllPaginatedAsync(page, pageSize);
            return Ok(categories);
        }

        // ─────────────── GET /api/categories/{id} ───────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateUpdateCategoryDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] CreateUpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}