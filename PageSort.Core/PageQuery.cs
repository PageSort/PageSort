using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PageSort.Core;

/// <summary>
/// Properties to be used during paging and sorting of a collection.
/// </summary>
public class PageQuery
{

    #region Helpers

    private int pageNumber { get; set; }

    #endregion

    public int PageNumber
    {
        get { return pageNumber; }
        set
        {
            pageNumber = (value <= 0) ? 1 : value;
        }
    }

    public int PageSize { get; set; }

    public IDictionary<string, string> Filters { get; } =
        new Dictionary<string, string>();

    public string[]? Fields { get; set; } = null;

    public string? SortProperty { get; set; }

    public ListSortDirection? SortDirection { get; set; } = ListSortDirection.Ascending;
}
