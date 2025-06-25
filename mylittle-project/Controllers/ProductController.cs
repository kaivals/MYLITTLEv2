
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;

namespace mylittle_project.API.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _productService.CreateAsync(dto);
            return Ok(new { message = "Product created successfully" });
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _productService.UpdateAsync(id, dto);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}