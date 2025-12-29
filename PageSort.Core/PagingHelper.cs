using PageSort.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PageSort.Core
{
    /// <summary>
    /// Provides methods for paging, sorting, and dynamically selecting fields from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public static class Page<T>
    {
        #region Sync Methods

        /// <summary>
        /// Pages and sorts a collection based on the provided <see cref="PageQuery"/>. 
        /// </summary>
        /// <param name="collection">The collection to page.</param>
        /// <param name="pageQuery">The paging and sorting parameters.</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the paged collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> or <paramref name="pageQuery"/> is null.</exception>
        public static PagedResult<T> GeneratePaging(IQueryable<T> collection, PageQuery pageQuery)
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(pageQuery);

            if (pageQuery.SortProperty is not null)
                collection = collection.OrderByProperty(pageQuery.SortProperty, pageQuery.SortDirection ?? ListSortDirection.Ascending);

            int totalCount = collection.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageQuery.PageSize);

            return new PagedResult<T>
            {
                CurrentPage = pageQuery.PageNumber,
                PageSize = pageQuery.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PreviousPage = pageQuery.PageNumber > 1,
                NextPage = pageQuery.PageNumber < totalPages,
                Collection = collection.Page(pageQuery.PageNumber, pageQuery.PageSize).ToList()
            };
        }


        /// <summary>
        /// Dynamically selects specified fields from the source collection, maps them to <typeparamref name="TDestination"/>, and pages the result.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TDestination">The type to map the selected fields to.</typeparam>
        /// <param name="collection">The source collection to page.</param>
        /// <param name="pageQuery">The paging parameters including requested fields.</param>
        /// <returns>A <see cref="PagedResult{TDestination}"/> containing the paged mapped items.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> or <paramref name="pageQuery"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no fields are provided or none match the destination type properties.</exception>
        public static PagedResult<TDestination> GeneratePaging<TSource, TDestination>(IQueryable<TSource> collection, AdvancedPageQuery pageQuery)
            where TDestination : class, new()
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(pageQuery);

            if (pageQuery.Fields is null or [])
            {
                pageQuery.Fields = [.. typeof(TSource).GetProperties().Select(p => p.Name)];
            }

            var fields = pageQuery.Fields
                .Select(f => f.Trim())
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            var destinationProperties = typeof(TDestination).GetProperties()
                .Select(p => p.Name)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            if (!destinationProperties.Any(p => fields.Contains(p, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("At least one destination property must be part of the selected fields.");

            if (pageQuery.Filters?.Count > 0)
            {
                collection = collection.ApplyFilters(pageQuery.Filters);
            }

            if (!string.IsNullOrEmpty(pageQuery.SortProperty) && !fields.Contains(pageQuery.SortProperty, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Sort field '{pageQuery.SortProperty}' must be part of the selected fields.");

            int totalCount = collection.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageQuery.PageSize);

            IQueryable<TDestination> projected = collection.ProjectToDestination<TSource, TDestination>([.. fields]);

            IQueryable<TDestination> pagedCollection = projected.Page(pageQuery.PageNumber, pageQuery.PageSize);

            return new PagedResult<TDestination>
            {
                CurrentPage = pageQuery.PageNumber,
                PageSize = pageQuery.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PreviousPage = pageQuery.PageNumber > 1,
                NextPage = pageQuery.PageNumber < totalPages,
                Collection = pagedCollection
            };
        }

        #endregion

        #region Async Versions

        /// <summary>
        /// Async version of <see cref="GeneratePaging(IQueryable{T}, PageQuery)"/>.
        /// </summary>
        public static Task<PagedResult<T>> GeneratePagingAsync(IQueryable<T> collection, PageQuery pageQuery)
            => Task.FromResult(GeneratePaging(collection, pageQuery));

        /// <summary>
        /// Async version of <see cref="GeneratePagingDynamic{TSource, TDestination}(IQueryable{TSource}, PageQuery)"/>.
        /// </summary>
        public static Task<PagedResult<TDestination>> GeneratePagingAsync<TSource, TDestination>(IQueryable<TSource> collection, AdvancedPageQuery pageQuery)
            where TDestination : class, new()
            => Task.FromResult(GeneratePaging<TSource, TDestination>(collection, pageQuery));

        #endregion
    }
}
