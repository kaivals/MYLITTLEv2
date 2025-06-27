using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
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
        // Purpose: Fetch paginated + filtered list of all categories (for category table)
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _categoryService.GetAllPaginatedAsync(page, pageSize);
            return Ok(result);
        }

        // ─────────────── POST /api/categories/filter ───────────────
        // Purpose: Filtered + sorted + paginated category list
        [HttpPost("filter")]
        public async Task<ActionResult<PaginatedResult<CategoryDto>>> Filter([FromBody] CategoryFilterDto filter)
        {
            var result = await _categoryService.GetFilteredAsync(filter);
            return Ok(result);
        }

        // ─────────────── GET /api/categories/{id} ───────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        // ─────────────── POST /api/categories ───────────────
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateUpdateCategoryDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ─────────────── PUT /api/categories/{id} ───────────────
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] CreateUpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        // ─────────────── DELETE /api/categories/{id} ───────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
