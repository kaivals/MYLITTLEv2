using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;

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
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Category = p.Category,
                    Brand = p.Brand,
                    Price = p.Price,
                    Stock = p.Stock,
                    Status = p.Status,
                    Description = p.Description,
                    TenantId = p.TenantId
                })
                .ToListAsync();
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new Exception("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Category = product.Category,
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
            var entity = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = dto.ProductName,
                Category = dto.Category,
                Brand = dto.Brand,
                Price = dto.Price,
                Stock = dto.Stock,
                Status = dto.Status,
                Description = dto.Description,
                TenantId = dto.TenantId
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, ProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new Exception("Product not found");

            product.ProductName = dto.ProductName;
            product.Category = dto.Category;
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
