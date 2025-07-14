using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.DTOs.Common;
using mylittle_project.Application.Interfaces;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // ✅ POST Endpoints
        /// <summary>Filter Categories (Paginated Search)</summary>
        [HttpPost("filter")]
        public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetFiltered([FromBody] BaseFilterDto filter)
        {
            var result = await _categoryService.GetFilteredAsync(filter);
            return Ok(result);
        }

        /// <summary>Create a New Category</summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateUpdateCategoryDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ GET Endpoints
        /// <summary>Get Category by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        // ✅ PUT/PATCH Endpoints
        /// <summary>Update Category</summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] CreateUpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        /// <summary>Soft Delete Category</summary>
        [HttpPatch("soft-delete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var success = await _categoryService.SoftDeleteCategoryAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
