using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Uncomment if you have authentication
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;

        public ProductController(IProductService productService, IProductAttributeService productAttributeService)
        {
            _productService = productService;
            _productAttributeService = productAttributeService;
        }

        // ──────────────── POST ENDPOINTS ────────────────

        [HttpPost("section")]
        public async Task<IActionResult> CreateSection([FromBody] ProductSectionDto dto)
        {
            var id = await _productService.CreateSectionAsync(dto);
            return Ok(new { SectionId = id });
        }

        [HttpPost("field")]
        public async Task<IActionResult> CreateField([FromBody] ProductFieldDto dto)
        {
            var id = await _productService.CreateFieldAsync(dto);
            return Ok(new { FieldId = id });
        }

        [HttpPost("product")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto dto)
        {
            var id = await _productService.AddNewProductAsync(dto);
            return Ok(new { ProductId = id });
        }

        [HttpPost("product-filter")]
        public async Task<IActionResult> FilterProducts([FromBody] ProductFilterRequest request)
        {
            var products = await _productService.FilterProductsAsync(request);

            if (!products.Any())
                return Ok(new List<object>()); // No products found

            if (request.Summary)
            {
                var summaries = products.Select(p => new ProductFilterSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandName = p.Brand?.Name,
                    AverageRating = p.Reviews?.Any() == true ? p.Reviews.Average(r => r.Rating) : null,
                    Price = p.Price,
                    Tags = p.Tags?.Select(t => t.Name).ToList(),
                    FieldValues = p.FieldValues?.ToDictionary(
                        fv => fv.Field?.Name ?? $"Field-{fv.FieldId}",
                        fv => fv.Value
                    )
                }).ToList();

                return Ok(summaries);
            }
            else
            {
                var details = products.Select(p => new ProductDetailsDto
                {
                    Name = p.Name,
                    Brand = p.Brand != null ? new BrandProductDto
                    {
                        Name = p.Brand.Name,
                        Description = p.Brand.Description,
                        Status = p.Brand.Status,
                        Order = p.Brand.Order,
                        Created = p.Brand.CreatedAt
                    } : null,
                    Tags = p.Tags?.Select(tag => new ProductTagDto
                    {
                        Name = tag.Name,
                        Published = tag.Published,
                        TaggedProducts = tag.TaggedProducts,
                        CreatedAt = tag.CreatedAt
                    }).ToList(),
                    Reviews = p.Reviews?.Select(review => new ProductReviewDto
                    {
                        Title = review.Title,
                        ReviewText = review.ReviewText,
                        Rating = review.Rating,
                        IsApproved = review.IsApproved,
                        IsVerified = review.IsVerified,
                        CreatedOn = review.CreatedOn
                    }).ToList(),
                    FieldValues = p.FieldValues?.ToDictionary(
                        fv => fv.Field?.Name ?? $"Field-{fv.FieldId}",
                        fv => fv.Value
                    )
                }).ToList();

                return Ok(details);
            }
        }

        // ──────────────── GET ENDPOINTS ────────────────

        [HttpGet("get-fields")]
        public async Task<IActionResult> GetProductFields()
        {
            var fields = await _productService.GetAllFieldsAsync();
            return Ok(fields);
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // ──────────────── PUT ENDPOINTS ────────────────

        [HttpPut("{id}/fields")]
        public async Task<IActionResult> UpdateProductFields(Guid id, [FromBody] Dictionary<string, string> fieldValues)
        {
            try
            {
                var success = await _productService.UpdateProductFieldsAsync(id, fieldValues);
                if (success)
                    return Ok("Fields updated successfully.");
                else
                    return BadRequest("No fields were provided.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ──────────────── DELETE & PATCH (Soft Delete / Restore) ────────────────

        [HttpDelete("product-fields/{fieldId}")]
        public async Task<IActionResult> SoftDeleteProductField(Guid fieldId)
        {
            var result = await _productService.SoftDeleteProductFieldAsync(fieldId);
            if (!result)
                return NotFound(new { message = "Product Field not found or already deleted." });

            return Ok(new { message = "Product Field soft-deleted successfully." });
        }

        [HttpDelete("product-sections/{sectionId}")]
        public async Task<IActionResult> SoftDeleteProductSection(Guid sectionId)
        {
            var result = await _productService.SoftDeleteProductSectionAsync(sectionId);
            if (!result)
                return NotFound(new { message = "Product Section not found or already deleted." });

            return Ok(new { message = "Product Section soft-deleted successfully." });
        }

        [HttpPatch("product-attributes/{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteProductAttribute(Guid id)
        {
            var result = await _productAttributeService.SoftDeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Product Attribute not found or already deleted." });

            return Ok(new { message = "Product Attribute soft-deleted successfully." });
        }

        [HttpPatch("product-attributes/{id}/restore")]
        public async Task<IActionResult> RestoreProductAttribute(Guid id)
        {
            var result = await _productAttributeService.RestoreAsync(id);
            if (!result)
                return NotFound(new { message = "Product Attribute not found or not deleted." });

            return Ok(new { message = "Product Attribute restored successfully." });
        }
    }
}
