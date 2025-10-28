using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PageSort.Common;

/// <summary>
/// Paging helper. 
/// </summary>
/// <typeparam name="T"></typeparam>
public static class Page<T>
{
    /// <summary>
    /// Pages and sorts the collection using pageQuery values.
    /// </summary>
    /// <param name="collection">Collection to be paged</param>
    /// <param name="pageQuery">PageQuery that has values to be used during paging.</param>
    /// <returns>PagedResult</returns>
    public static PagedResult<T> GeneratePaging(IQueryable<T> collection, PageQuery pageQuery)
    {
        ArgumentNullException.ThrowIfNull(pageQuery);
        ArgumentNullException.ThrowIfNull(collection);

        int count = collection.Count();
        int CurrentPage = pageQuery.PageNumber;
        int PageSize = pageQuery.PageSize;
        int TotalCount = count;
        int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

        if (pageQuery.SortProperty is not null)
            collection = collection.OrderByProperty(pageQuery.SortProperty, pageQuery.SortDirection ?? ListSortDirection.Ascending);

        return new PagedResult<T>
        {
            CurrentPage = CurrentPage,
            NextPage = CurrentPage < TotalPages,
            PageSize = PageSize,
            PreviousPage = CurrentPage > 1,
            Collection = [.. collection.Page(CurrentPage, PageSize)],
            TotalCount = TotalCount,
            TotalPages = TotalPages
        };
    }

    public static Task<PagedResult<T>> GeneratePagingAsync(IQueryable<T> collection, PageQuery pageQuery)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(pageQuery);

        int count = collection.Count();
        int currentPage = pageQuery.PageNumber;
        int pageSize = pageQuery.PageSize;
        int totalCount = count;
        int totalPages = (int)Math.Ceiling(count / (double)pageSize);

        if (pageQuery.SortProperty is not null)
            collection = collection.OrderByProperty(pageQuery.SortProperty, pageQuery.SortDirection ?? ListSortDirection.Ascending);


        return Task.FromResult(new PagedResult<T>
        {
            CurrentPage = currentPage,
            NextPage = currentPage < totalPages,
            PageSize = pageSize,
            PreviousPage = currentPage > 1,
            Collection = [.. collection.Page(currentPage, pageSize)],
            TotalCount = totalCount,
            TotalPages = totalPages
        });

    }
}
