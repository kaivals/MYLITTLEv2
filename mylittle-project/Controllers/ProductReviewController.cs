﻿using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductReviewController : ControllerBase
    {
        private readonly IProductReviewService _service;

        public ProductReviewController(IProductReviewService service)
        {
            _service = service;
        }

        // ──────────────── POST ENDPOINTS ────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductReviewDto dto)
        {
            var review = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            var result = await _service.BulkDeleteAsync(ids);
            return result
                ? Ok(new { Message = "Reviews deleted successfully." })
                : NotFound(new { Message = "Some or all review IDs were not found." });
        }

        // ──────────────── GET ENDPOINTS ────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var review = await _service.GetByIdAsync(id);
            if (review == null)
                return NotFound();
            return Ok(review);
        }

        // ──────────────── PUT ENDPOINTS ────────────────

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductReviewDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        // ──────────────── PATCH ENDPOINTS ────────────────

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _service.RestoreAsync(id);
            return result
                ? Ok(new { Message = "Review restored successfully." })
                : NotFound(new { Message = "Review not found or not deleted." });
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _service.ApproveAsync(id);
            return result
                ? Ok(new { Message = "Review approved successfully." })
                : NotFound();
        }

        [HttpPatch("{id}/disapprove")]
        public async Task<IActionResult> Disapprove(Guid id)
        {
            var result = await _service.DisapproveAsync(id);
            return result
                ? Ok(new { Message = "Review disapproved successfully." })
                : NotFound();
        }

        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> Verify(Guid id)
        {
            var result = await _service.VerifyAsync(id);
            return result
                ? Ok(new { Message = "Review verified successfully." })
                : NotFound();
        }

        // ──────────────── DELETE ENDPOINTS ────────────────

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
