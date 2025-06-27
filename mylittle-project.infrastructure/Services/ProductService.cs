using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using MyProject.Application.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category) // Include Category
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Category = p.Category.Name, // Use Category.Name
                    Brand = p.Brand,
                    Price = p.Price,
                    Stock = p.Stock,
                    Status = p.Status,
                    Description = p.Description,
                    TenantId = p.TenantId
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<ProductDto>> GetPaginatedAsync(int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Category = p.Category.Name,
                    Brand = p.Brand,
                    Price = p.Price,
                    Stock = p.Stock,
                    Status = p.Status,
                    Description = p.Description,
                    TenantId = p.TenantId
                });

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<ProductDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new Exception("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Category = product.Category.Name,
                Brand = product.Brand,
                Price = product.Price,
                Stock = product.Stock,
                Status = product.Status,
                Description = product.Description,
                TenantId = product.TenantId
            };
        }

        public async Task CreateAsync(ProductDto dto)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                throw new Exception("Category not found");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = dto.ProductName,
                CategoryId = dto.CategoryId,
                Brand = dto.Brand,
                Price = dto.Price,
                Stock = dto.Stock,
                Status = dto.Status,
                Description = dto.Description,
                TenantId = dto.TenantId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

        }

        public async Task UpdateAsync(Guid id, ProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new Exception("Product not found");

            var category = await _context.Categories.FindAsync(dto.CategoryId); // ✅ NO Guid.Parse

            if (category == null)
                throw new Exception("Category not found");

            product.ProductName = dto.ProductName;
            product.CategoryId = category.Id;
            product.Brand = dto.Brand;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Status = dto.Status;
            product.Description = dto.Description;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new Exception("Product not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
