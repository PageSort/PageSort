using System;
using System.Collections.Generic;
using System.Text;

namespace PageSort
{
    public class PagedResult<TEntity>
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public IList<TEntity> Collection { get; set; }

        public bool PreviousPage { get; set; }

        public bool NextPage { get; set; }
    }
}
