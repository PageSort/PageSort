using PageSort.Core.Enums;
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

    public string? SortProperty { get; set; }

    public ListSortDirection? SortDirection { get; set; } = ListSortDirection.Ascending;
}


public sealed class AdvancedPageQuery : PageQuery
{
    public List<Filter> Filters { get; set; } = [];

    public string[]? Fields { get; set; } = null;

}

/// <summary>
/// Represents a single filter condition.
/// </summary>
public sealed class Filter
{
    /// <summary>
    /// The property name to filter on.
    /// </summary>
    public string Field { get; } = null!;
    /// <summary>
    /// The comparison operator. Examples: 'Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'Contains', 'StartsWith', 'EndsWith'.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when an invalid operator string is set.</exception>
    public string Operator { get; } = "=";


    /// <summary>
    /// The value to compare the field to.
    /// </summary>
    public object Value { get; } = null!;

    private Filter(string field, OperatorType @operator, object value)
    {
        Operator = @operator switch
        {
            OperatorType.Equals => "=",
            OperatorType.NotEquals => "!=",
            OperatorType.GreaterThan => ">",
            OperatorType.LessThan => "<",
            OperatorType.GreaterThanOrEqual => ">=",
            OperatorType.LessThanOrEqual => "<=",
            OperatorType.Contains => "Contains",
            OperatorType.StartsWith => "StartsWith",
            OperatorType.EndsWith => "EndsWith",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null)
        };
        Field = field;
        Value = value;
    }

    public static Filter Create(string field, OperatorType @operator, object value)
    {
        return new Filter(field, @operator, value);
    }

    private static void ValidateOperator(object value, OperatorType op)
    {
        var valueType = value?.GetType() ?? throw new ArgumentNullException(nameof(value));
        bool isString = valueType == typeof(string) && IsPureString((string)value);
        bool isComparable = typeof(IComparable).IsAssignableFrom(valueType);

        switch (op)
        {
            case OperatorType.Equals:
            case OperatorType.NotEquals:
                break;

            case OperatorType.GreaterThan:
            case OperatorType.GreaterThanOrEqual:
            case OperatorType.LessThan:
            case OperatorType.LessThanOrEqual:
                if (!isComparable)
                    throw new InvalidOperationException(
                        $"Operator '{op}' is not supported for type '{valueType.Name}'");
                break;

            case OperatorType.Contains:
            case OperatorType.StartsWith:
            case OperatorType.EndsWith:
                if (!isString)
                    throw new InvalidOperationException(
                        $"Operator '{op}' is only valid for strings, not '{valueType.Name}'");
                break;

            default:
                throw new InvalidOperationException($"Unsupported operator '{op}'");
        }
    }

    static bool IsPureString(string _value)
    {
        return !string.IsNullOrWhiteSpace(_value) &&
           _value.All(char.IsLetter);
    }
}
