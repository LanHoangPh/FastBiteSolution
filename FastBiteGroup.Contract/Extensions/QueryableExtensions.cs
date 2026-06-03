using FastBiteGroup.Contract.Abstractions.Shared;
using Microsoft.EntityFrameworkCore;
namespace FastBiteGroup.Application.Exception;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageIndex = pageIndex <= 0 ? PagedResult<T>.DefaultPageIndex : pageIndex;
        pageSize = pageSize <= 0 ? PagedResult<T>.DefaultPageSize :
                   pageSize > PagedResult<T>.UpperPageSize ? PagedResult<T>.UpperPageSize : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageIndex, pageSize, totalCount);
    }

}
