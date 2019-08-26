using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PageSort
{
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
            int count = collection.Count();
            int CurrentPage = pageQuery.PageNumber;
            int PageSize = pageQuery.PageSize;
            int TotalCount = count;
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            if (!string.IsNullOrEmpty(pageQuery.SortProperty))
            {
                if (pageQuery.SortDirection == ListSortDirection.Descending)
                    collection = collection.OrderByDescendingProperty(pageQuery.SortProperty);

                else
                    collection = collection.OrderByProperty(pageQuery.SortProperty);
            }
            
            var PreviousPage = CurrentPage > 1 ? true : false;
            var NextPage = CurrentPage < TotalPages ? true : false;

            return new PagedResult<T>
            {
                CurrentPage = CurrentPage,
                NextPage = NextPage,
                PageSize = PageSize,
                PreviousPage = PreviousPage,
                Collection = collection.Page(CurrentPage, PageSize),
                TotalCount = TotalCount,
                TotalPages = TotalPages
            };
        }
    }
}
