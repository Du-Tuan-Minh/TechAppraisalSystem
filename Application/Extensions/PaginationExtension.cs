using Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions
{
    public static class PaginationExtension
    {
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(this IQueryable<T> query, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalCount = await query.CountAsync(); 
            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
