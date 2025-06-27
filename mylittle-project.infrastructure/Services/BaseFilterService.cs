using Microsoft.EntityFrameworkCore;
using MyProject.Application.DTOs;
using MyProject.Application.DTOs.Common;
using MyProject.Application.Interfaces.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyProject.Infrastructure.Services.Common
{
    public abstract class BaseFilterService<TEntity, TDto, TFilterDto> : IBaseFilterService<TDto, TFilterDto>
        where TEntity : class
        where TFilterDto : BaseFilterDto
    {
        protected readonly DbContext _context;

        protected BaseFilterService(DbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<TDto>> GetFilteredAsync(TFilterDto filter)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsQueryable();

            query = ApplyBaseFilters(query, filter);

            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapToDto())
                .ToListAsync();

            return new PaginatedResult<TDto>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems
            };
        }

        protected virtual IQueryable<TEntity> ApplyBaseFilters(IQueryable<TEntity> query, TFilterDto filter)
        {
            return query;
        }

        protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string? sortBy, string? sortDir)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            var isAsc = sortDir?.ToLower() == "asc";

            try
            {
                var param = Expression.Parameter(typeof(TEntity), "x");
                var property = Expression.PropertyOrField(param, sortBy);
                var lambda = Expression.Lambda(property, param);

                var methodName = isAsc ? "OrderBy" : "OrderByDescending";
                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2);

                var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);

                return (IQueryable<TEntity>)genericMethod.Invoke(null, new object[] { query, lambda })!;
            }
            catch
            {
                return query; // fallback
            }
        }

        protected abstract Expression<Func<TEntity, TDto>> MapToDto();
    }
}
