using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureAccessService _featureAccessService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _context;

        public ProductService(
            IUnitOfWork unitOfWork,
            IFeatureAccessService featureAccessService,
            AppDbContext context,
            IHttpContextAccessor httpContext)
        {
            _unitOfWork = unitOfWork;
            _featureAccessService = featureAccessService;
            _httpContext = httpContext;
            _context = context;
        }

        private Guid GetTenantId()
        {
            var tenantId = _httpContext.HttpContext?.Request.Headers["Tenant-ID"].FirstOrDefault();
            if (tenantId == null)
                throw new UnauthorizedAccessException("Tenant ID not found in header.");

            return Guid.Parse(tenantId);
        }

        private string GetSqlColumnType(string fieldType)
        {
            return fieldType.ToLower() switch
            {
                "string" => "NVARCHAR(MAX)",
                "int" => "INT",
                "decimal" => "DECIMAL(18,2)",
                "bool" => "BIT",
                "datetime" => "DATETIME",
                _ => "NVARCHAR(MAX)"
            };
        }


        public async Task<int> ResyncProductFieldValuesAsync(Guid? tenantId = null)
        {
            tenantId ??= GetTenantId();  // Use header if not passed manually

            var fields = await _context.ProductFields
                .Where(f => f.TenantId == tenantId)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .ToListAsync();

            int totalInserted = 0;
            int totalUpdated = 0;

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            foreach (var product in products)
            {
                var fieldValues = new Dictionary<string, string>();

                // Read full product row to fetch dynamic columns
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM Products WHERE Id = '{product.Id}'";
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    foreach (var field in fields)
                    {
                        var value = reader[field.Name]?.ToString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            fieldValues[field.Name] = value;
                        }
                    }
                }

                reader.Close();  // Important: Close reader before next iteration

                // Insert / Update product field values
                foreach (var kv in fieldValues)
                {
                    var field = fields.FirstOrDefault(f => f.Name == kv.Key);
                    if (field == null) continue;

                    var existing = await _context.ProductFieldValues
                        .FirstOrDefaultAsync(pfv =>
                            pfv.ProductId == product.Id &&
                            pfv.FieldId == field.Id &&
                            pfv.TenantId == tenantId);

                    if (existing == null)
                    {
                        var newValue = new ProductFieldValue
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            FieldId = field.Id,
                            TenantId = tenantId.Value,
                            Value = kv.Value,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _context.ProductFieldValues.AddAsync(newValue);
                        totalInserted++;
                    }
                    else if (existing.Value != kv.Value)  // Update only if value changed
                    {
                        existing.Value = kv.Value;
                        existing.UpdatedAt = DateTime.UtcNow;
                        totalUpdated++;
                    }
                }

                Console.WriteLine($"✅ Processed product: {product.Name} ({product.Id})");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Resync complete. Inserted: {totalInserted}, Updated: {totalUpdated}");
            return totalInserted;
        }




        public async Task<List<Product>> FilterProductsAsync(ProductFilterRequest request)
        {
            var tenantId = GetTenantId();

            // Start with all tenant products (soft-deleted excluded)
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Tags)
                .Include(p => p.Reviews)
                .Include(p => p.Categories)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .AsQueryable();

            // ✅ Category Filter
            if (!string.IsNullOrWhiteSpace(request.CategoryName))
            {
                query = query.Where(p => p.Categories.Any(c => c.Name.Contains(request.CategoryName)));
            }

            // ✅ Brand Filter
            if (!string.IsNullOrWhiteSpace(request.BrandName))
            {
                query = query.Where(p => p.Brand != null && p.Brand.Name.Contains(request.BrandName));
            }


            // ✅ Tags Filter
            if (request.Tags != null && request.Tags.Any(t => !string.IsNullOrWhiteSpace(t)))
            {
                query = query.Where(p => p.Tags.Any(tag => request.Tags.Contains(tag.Name)));
            }

            // ✅ Price Filter
            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice);
            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice);

            // ✅ Rating Filter
            if (request.MinRating.HasValue)
            {
                query = query.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) >= request.MinRating);
            }

            // ✅ Field Values Filter
            if (request.FieldValues != null && request.FieldValues.Any())
            {
                var fields = await _context.ProductFields
                    .Where(f => f.TenantId == tenantId && request.FieldValues.Keys.Contains(f.Name))
                    .ToListAsync();

                var fieldNameToId = fields.ToDictionary(f => f.Name, f => f.Id);

                if (fieldNameToId.Any())
                {
                    var allFieldValues = await _context.ProductFieldValues
                        .Where(pfv => pfv.TenantId == tenantId && fieldNameToId.Values.Contains(pfv.FieldId))
                        .ToListAsync();

                    var ids = allFieldValues
                        .Where(pfv =>
                            request.FieldValues.TryGetValue(
                                fields.First(f => f.Id == pfv.FieldId).Name, out var values) &&
                            values.Any(v => !string.IsNullOrWhiteSpace(v)) &&  // Skip empty filters
                            values.Any(v => !string.IsNullOrWhiteSpace(pfv.Value) &&
                                            pfv.Value.Contains(v, StringComparison.OrdinalIgnoreCase)))
                        .Select(pfv => pfv.ProductId)
                        .Distinct()
                        .ToList();

                    if (ids.Any())
                    {
                        query = query.Where(p => ids.Contains(p.Id));
                    }
                    else
                    {
                        // No matching products on fields
                        return new List<Product>();
                    }
                }
            }

            return await query.ToListAsync();
        }









        public async Task<List<ProductFieldDto>> GetAllFieldsAsync()
        {
            var tenantId = GetTenantId();

            var fields = await _unitOfWork.ProductFields
                .GetAll()
                .Where(f => f.TenantId == tenantId && f.IsVisible)
                .Join(_unitOfWork.ProductSections.GetAll(),
                      field => field.SectionId,
                      section => section.Id,
                      (field, section) => new ProductFieldDto
                      {
                          Id = field.Id,
                          SectionId = field.SectionId,
                          Name = field.Name,
                          FieldType = field.FieldType,
                          IsRequired = field.IsRequired,
                          AutoSyncEnabled = field.AutoSyncEnabled,
                          IsVisibleToDealer = field.IsVisibleToDealer,
                          IsFilterable = field.IsFilterable,
                          IsVariantOption = field.IsVariantOption,
                          IsVisible = field.IsVisible,
                          Options = field.Options,
                          SectionName = section.Name // 💡 Added here
                      })
                .ToListAsync();

            return fields;
        }

        public async Task<ProductDetailsDto?> GetProductAsync(Guid id)
        {
            var tenantId = GetTenantId();

            var product = await _unitOfWork.Products
                .GetAll()
                .Include(p => p.Tags)
                .Include(p => p.Reviews)
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null)
                return null;

            var dto = new ProductDetailsDto
            {
                Name = product.Name,
                Price = product.Price,
                Categories = product.Categories?.Select(c => c.Name).ToList(),

                Brand = product.Brand != null ? new BrandProductDto
                {
                    Name = product.Brand.Name,
                    Description = product.Brand.Description,
                    Status = product.Brand.Status,
                    Order = product.Brand.Order,
                    Created = product.Brand.CreatedAt

                } : null,

                Tags = product.Tags?.Select(tag => new ProductTagDto
                {
                    Name = tag.Name,
                    Published = tag.Published,
                    TaggedProducts = tag.TaggedProducts,
                    CreatedAt = tag.CreatedAt
                }).ToList(),

                Reviews = product.Reviews?.Select(review => new ProductReviewDto
                {
                    Title = review.Title,
                    ReviewText = review.ReviewText,
                    Rating = review.Rating,
                    IsApproved = review.IsApproved,
                    IsVerified = review.IsVerified,
                    CreatedOn = review.CreatedOn
                }).ToList()
            };

            var excludedColumns = new[]
            {
        "Id", "TenantId", "Name", "CategoryId", "CreatedAt", "UpdatedAt", "BrandId",
        "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt",
        "BrandingTextId", "BrandingMediaId", "BrandingColorId", "Price"
    };

            var activeFields = await GetActiveFieldNamesAsync(tenantId);

            var fieldValues = new Dictionary<string, string>();
            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM Products WHERE Id = '{id}'";

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var column = reader.GetName(i);
                        if (!excludedColumns.Contains(column) && activeFields.Contains(column))
                        {
                            var value = reader[column]?.ToString() ?? string.Empty;
                            fieldValues[column] = value;
                        }
                    }
                }
            }

            dto.FieldValues = fieldValues;
            return dto;
        }



        public async Task<Guid> CreateSectionAsync(ProductSectionDto dto)
        {
            var tenantId = GetTenantId();

            if (!await _featureAccessService.IsFeatureEnabledAsync(tenantId, "products"))
                throw new UnauthorizedAccessException("Product feature not enabled for this tenant.");

            var section = new ProductSection
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductSections.AddAsync(section);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return section.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Guid> CreateFieldAsync(ProductFieldDto dto)
        {
            var tenantId = GetTenantId();

            if (!await _featureAccessService.IsFeatureEnabledAsync(tenantId, "products"))
                throw new UnauthorizedAccessException("Product feature not enabled for this tenant.");

            var fieldId = Guid.NewGuid();

            var field = new ProductField
            {
                Id = fieldId,
                TenantId = tenantId,
                SectionId = dto.SectionId,
                Name = dto.Name,
                FieldType = dto.FieldType,
                IsVisibleToDealer = dto.IsVisibleToDealer,
                IsRequired = dto.IsRequired,
                AutoSyncEnabled = dto.AutoSyncEnabled,
                IsFilterable = dto.IsFilterable,
                IsVariantOption = dto.IsVariantOption,
                IsVisible = dto.IsVisible,
                Options = dto.Options,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var columnName = field.Name;
            var columnType = GetSqlColumnType(field.FieldType);
            var alterTableSql = $"ALTER TABLE Products ADD [{columnName}] {columnType} NULL;";

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.ProductFields.AddAsync(field);
                await _context.Database.ExecuteSqlRawAsync(alterTableSql);

                if (field.AutoSyncEnabled)
                {
                    var attribute = new ProductAttribute
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Name = field.Name,
                        FieldType = field.FieldType,
                        Options = field.Options != null ? string.Join(",", field.Options) : null,
                        IsRequired = field.IsRequired,
                        IsVisible = true,
                        Source = "AutoSync",
                        SectionType = "Info",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ProductAttributes.AddAsync(attribute);
                }

                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                return fieldId;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task<bool> UpdateProductAsync(Guid productId, ProductDto dto)
        {
            var tenantId = GetTenantId();

            var product = await _unitOfWork.Products
                .GetAll()
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);

            if (product == null)
                throw new Exception("Product not found or access denied.");

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.BrandId = dto.BrandId;
            product.UpdatedAt = DateTime.UtcNow;

            // ✅ Update Categories
            if (dto.CategoryIds != null)
            {
                var categories = await _unitOfWork.Categories
                    .Find(c => dto.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                product.Categories.Clear();
                foreach (var category in categories)
                {
                    product.Categories.Add(category);
                }
            }

            // ✅ Update Tags (Optional)
            if (dto.TagIds != null)
            {
                var tags = await _unitOfWork.ProductTags
                    .Find(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();

                product.Tags.Clear();
                foreach (var tag in tags)
                {
                    product.Tags.Add(tag);
                }
            }

            // ✅ Update Dynamic Field Values (Optional)
            if (dto.FieldValues != null && dto.FieldValues.Any())
            {
                await UpdateFieldValuesInBothPlacesAsync(productId, tenantId, dto.FieldValues);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }




        public async Task<bool> UpdateProductFieldsAsync(Guid productId, Dictionary<string, string> fieldValues)
        {
            var tenantId = GetTenantId();

            var product = await _unitOfWork.Products
                .GetAll()
                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);

            if (product == null)
                throw new Exception("Product not found or access denied.");

            if (fieldValues == null || !fieldValues.Any())
                return false;

            await UpdateFieldValuesInBothPlacesAsync(productId, tenantId, fieldValues);

            return true;
        }





        private async Task UpdateFieldValuesInBothPlacesAsync(Guid productId, Guid tenantId, Dictionary<string, string> fieldValues)
        {
            var setClauses = new List<string>();

            foreach (var kv in fieldValues)
            {
                var column = kv.Key;
                var value = kv.Value?.Replace("'", "''") ?? "";
                setClauses.Add($"[{column}] = '{value}'");

                // Update or insert into ProductFieldValue table
                var field = await _unitOfWork.ProductFields.GetAll()
                    .FirstOrDefaultAsync(f => f.Name == column && f.TenantId == tenantId);

                if (field != null)
                {
                    var existing = await _unitOfWork.ProductFieldValues
                        .Find(pfv => pfv.ProductId == productId && pfv.FieldId == field.Id)
                        .FirstOrDefaultAsync();

                    if (existing != null)
                    {
                        existing.Value = kv.Value ?? "";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newValue = new ProductFieldValue
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            FieldId = field.Id,
                            TenantId = tenantId,
                            Value = kv.Value ?? "",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.ProductFieldValues.AddAsync(newValue);
                    }
                }
            }

            var updateQuery = $"UPDATE Products SET {string.Join(", ", setClauses)} WHERE Id = '{productId}'";
            await _context.Database.ExecuteSqlRawAsync(updateQuery);
            await _unitOfWork.SaveAsync();
        }



        // Service Layer
        public async Task<bool> SoftDeleteProductFieldAsync(Guid fieldId)
        {
            var tenantId = GetTenantId();  // Ensure multi-tenancy isolation
            var result = await _unitOfWork.ProductFields.SoftDeleteAsync(fieldId, tenantId);
            return result;
        }

        public async Task<bool> SoftDeleteProductSectionAsync(Guid sectionId)
        {
            var tenantId = GetTenantId();  // Ensure multi-tenancy isolation
            var result = await _unitOfWork.ProductSections.SoftDeleteAsync(sectionId, tenantId);
            return result;
        }

        public async Task<Guid> AddNewProductAsync(ProductDto dto)
        {
            var tenantId = GetTenantId();

            if (!await _featureAccessService.IsFeatureEnabledAsync(tenantId, "products"))
                throw new UnauthorizedAccessException("Product feature not enabled for this tenant.");

            if (dto.BrandId != null)
            {
                var brandExists = await _unitOfWork.Brands.GetAll()
                    .AnyAsync(b => b.Id == dto.BrandId);
                if (!brandExists)
                    throw new ArgumentException("Provided brand does not exist.");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = dto.Name,
                Price = dto.Price,
                BrandId = dto.BrandId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ✅ Add Categories
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var categories = await _unitOfWork.Categories
                    .Find(c => dto.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var category in categories)
                {
                    product.Categories.Add(category);
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveAsync();

                // ✅ Add Tags
                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    var tags = await _unitOfWork.ProductTags
                        .Find(t => dto.TagIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var tag in tags)
                    {
                        product.Tags.Add(tag);
                    }
                }



                // ✅ Add Dynamic Field Values
                if (dto.FieldValues != null && dto.FieldValues.Any())
                {
                    await UpdateFieldValuesInBothPlacesAsync(product.Id, tenantId, dto.FieldValues);
                }

                await _unitOfWork.CommitTransactionAsync();
                return product.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<List<string>> GetActiveFieldNamesAsync(Guid tenantId)
        {
            return await _context.ProductFields
                .Where(f => f.TenantId == tenantId && !f.IsDeleted)
                .Select(f => f.Name)
                .ToListAsync();
        }


    }
}
