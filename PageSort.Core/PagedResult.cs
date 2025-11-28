namespace PageSort.Core;

public class PagedResult<TEntity>
{
    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public IEnumerable<TEntity>? Collection { get; set; }

    public bool PreviousPage { get; set; }

    public bool NextPage { get; set; }
}
