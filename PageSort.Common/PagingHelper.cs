using PageSort.Common.Extensions;
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

        if (!string.IsNullOrEmpty(pageQuery.Fields))
            throw new InvalidOperationException(
                "Dynamic field selection requires GeneratePagingDynamic().");

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

    public static PagedResult<KeyValuePair<string, object?>> GeneratePagingDynamic<TSource>(IQueryable<TSource> collection, PageQuery pageQuery)
    {
        ArgumentNullException.ThrowIfNull(pageQuery);
        ArgumentNullException.ThrowIfNull(collection);

        if (string.IsNullOrWhiteSpace(pageQuery.Fields))
            throw new InvalidOperationException(
                "Fields must be provided when calling GeneratePagingDynamic().");

        var fields = pageQuery.Fields!
            .Split(',')
            .Select(f => f.Trim())
            .ToArray() ?? [];

        if (!string.IsNullOrEmpty(pageQuery.SortProperty) && !fields.Contains(pageQuery.SortProperty, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Sort field '{pageQuery.SortProperty}' must be part of the selected fields.");
        }

        var projectedQuery = collection.SelectDynamic(fields);

        int totalCount = collection.Count();
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageQuery.PageSize);

        var items = projectedQuery
            .Page(pageQuery.PageNumber, pageQuery.PageSize);

        return new PagedResult<KeyValuePair<string, object?>>
        {
            CurrentPage = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PreviousPage = pageQuery.PageNumber > 1,
            NextPage = pageQuery.PageNumber < totalPages,
            Collection = items.ToEnumerable()
        };
    }

    public static PagedResult<TDestination> GeneratePagingDynamic<TSource, TDestination>(IQueryable<TSource> collection, PageQuery pageQuery)
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(pageQuery);
        ArgumentNullException.ThrowIfNull(collection);

        if (string.IsNullOrWhiteSpace(pageQuery.Fields))
            throw new InvalidOperationException(
                "Fields must be provided when calling GeneratePagingDynamic().");

        var fields = pageQuery.Fields!
            .Split(',')
            .Select(f => f.Trim())
            .ToArray() ?? [];

        var destinationProperties = typeof(TDestination).GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!destinationProperties.Any(p => fields.Contains(p, StringComparer.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "At least one of the destination type properties must be part of the selected fields.");
        }

        if (!string.IsNullOrEmpty(pageQuery.SortProperty) && !fields.Contains(pageQuery.SortProperty, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Sort field '{pageQuery.SortProperty}' must be part of the selected fields.");
        }

        if (pageQuery.SortProperty is not null)
            collection = collection.OrderByProperty(pageQuery.SortProperty, pageQuery.SortDirection ?? ListSortDirection.Ascending);


        var projectedQuery = collection.SelectDynamic(fields);

        int totalCount = collection.Count();
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageQuery.PageSize);

        var items = projectedQuery
            .Page(pageQuery.PageNumber, pageQuery.PageSize);

        return new PagedResult<TDestination>
        {
            CurrentPage = pageQuery.PageNumber,
            PageSize = pageQuery.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PreviousPage = pageQuery.PageNumber > 1,
            NextPage = pageQuery.PageNumber < totalPages,
            Collection = items.MapTo<TDestination>()
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
