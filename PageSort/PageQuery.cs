using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PageSort
{
    public class PageQuery
    {

        #region Helpers

        const int maxPageSize = 20;

        private int _pageNumber { get; set; }

        private int _pageSize { get; set; } = 10;

        #endregion

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = (value <= 0) ? 1 : value;
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > maxPageSize || value <= 0) ? maxPageSize : value; }
        }

        public IDictionary<string, string> Filters { get; } =
            new Dictionary<string, string>();

        public string SortProperty { get; set; }

        public ListSortDirection? SortDirection { get; set; }
    }
}
