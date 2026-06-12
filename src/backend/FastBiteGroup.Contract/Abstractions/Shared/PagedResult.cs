namespace FastBiteGroup.Contract.Abstractions.Shared;

public class PagedResult<T>(List<T> items, int pageIndex, int pageSize, int totalCount)
{
    public const int UpperPageSize = 100;
    public const int DefaultPageSize = 10;
    public const int DefaultPageIndex = 1;

    public List<T> Items { get; } = items;
    public int PageIndex { get; } = pageIndex;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;

    public bool HasNextPage => PageIndex * PageSize < TotalCount;
    public bool HasPreviousPage => PageIndex > 1;
}
