using PageSort.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PageSort.Core.Extensions;

/// <summary>
/// 
/// </summary>
public static class IQueryableExtension
{
    private static readonly MethodInfo OrderByMethod = typeof(Queryable)
        .GetMethods()
        .Where(method => method.Name == Constants.Ordering.OrderBy)
        .Single(method => method.GetParameters().Length == 2);

    private static readonly MethodInfo OrderByDescendingMethod = typeof(Queryable)
        .GetMethods()
        .Where(method => method.Name == Constants.Ordering.OrderByDescending)
        .Single(method => method.GetParameters().Length == 2);

    /// <summary>
    /// Returns page collection of the specified source query.
    /// </summary>
    /// <param name="source">Source query</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Ordered collection</returns>
    public static IQueryable<T> Page<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("The value provided can neither be a zero or a negative", nameof(pageNumber));
        }

        if (pageSize < 0)
        {
            throw new ArgumentException("The value provided can not be a negative", nameof(pageSize));
        }

        return source.Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize);
    }

    /// <summary>
    /// Returns source ordered by the specified property and sort Direction.
    /// </summary>
    /// <param name="source">Source query</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="sortDirection">Sort Direction</param>
    /// <returns>Ordered collection</returns>
    public static IQueryable<TSource> OrderByProperty<TSource>
        (this IQueryable<TSource> source, string propertyName, ListSortDirection? sortDirection = ListSortDirection.Ascending)
    {
        var orderByProperty = GetOrderByExpression<TSource>(propertyName, out LambdaExpression lambda);

        return sortDirection switch
        {
            ListSortDirection.Descending => 
                GetSortedSource(source, OrderByDescendingMethod.MakeGenericMethod(typeof(TSource), orderByProperty.Type), lambda),

            _ => GetSortedSource(source, OrderByMethod.MakeGenericMethod(typeof(TSource), orderByProperty.Type), lambda)
        };

    }

    /// <summary>
    /// Projects the source query into a new form by selecting only the specified
    /// property names at runtime. This enables clients to request partial data
    /// representations dynamically (e.g., through query parameters).
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of the elements in the source query.
    /// </typeparam>
    /// <param name="source">
    /// The <see cref="IQueryable{T}"/> to apply the projection to.
    /// </param>
    /// <param name="fields">
    /// A collection of property names to include in the projection.
    /// Each name must match a public property on <typeparamref name="TSource"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> representing the projected result,
    /// containing only the specified fields.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more specified fields do not exist on
    /// <typeparamref name="TSource"/>.
    /// </exception>
    /// <example>
    /// <para>
    /// The following example demonstrates how to return only selected fields
    /// from an entity based on client input:
    /// </para>
    /// <code>
    /// // GET /api/users?fields=Id,Name
    /// var fields = new[] { "Id", "Name" };
    ///
    /// var result = _context.Users
    ///                      .SelectDynamic(fields)
    ///                      .ToList();
    /// </code>
    /// <para>
    /// The resulting objects will contain only the <c>Id</c> and <c>Name</c>
    /// properties for each user.
    /// </para>
    /// </example>
    public static IQueryable<Dictionary<string, object>> SelectDynamic<TSource>(this IQueryable<TSource> source, string[] fields)
    {
        ValidateFieldsAreAllowed<TSource>(fields);

        var invalid = fields.Where(f =>
            typeof(TSource).GetProperty(f,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) == null)
            .ToList();

        if (invalid.Count != 0)
            throw new ArgumentException($"Unknown field(s): {string.Join(", ", invalid)}");

        var parameter = Expression.Parameter(typeof(TSource), "x");

        var dictCtor = typeof(Dictionary<string, object>)
            .GetConstructor(Type.EmptyTypes)!;

        var dictVar = Expression.Variable(typeof(Dictionary<string, object>), "dict");
        var addMethod = typeof(Dictionary<string, object>).GetMethod("Add")!;

        var block = new List<Expression>
        {
            Expression.Assign(dictVar, Expression.New(dictCtor))
        };

        foreach (var field in fields)
        {
            var prop = typeof(TSource).GetProperty(field,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;

            var access = Expression.Convert(Expression.Property(parameter, prop), typeof(object));

            block.Add(Expression.Call(dictVar, addMethod, Expression.Constant(prop.Name), access));
        }

        block.Add(dictVar);

        var selector = Expression.Lambda<Func<TSource, Dictionary<string, object>>>(
            Expression.Block([dictVar], block),
            parameter);

        return source.Select(selector);
    }


    /// <summary>
    /// Applies dynamic filters to an IQueryable based on a list of Filter objects.
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, IEnumerable<Filter> filters)
    {
        if (filters == null || !filters.Any()) return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        

        Expression? predicate = null;

        foreach (var filter in filters)
        {
            var member = Expression.Property(parameter, filter.Field);

            Expression condition = filter.Operator switch
            {
                "=" => Expression.Equal(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                "!=" => Expression.NotEqual(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                ">" => Expression.GreaterThan(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                ">=" => Expression.GreaterThanOrEqual(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                "<" => Expression.LessThan(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                "<=" => Expression.LessThanOrEqual(member, Expression.Constant(Convert.ChangeType(filter.Value, member.Type))),
                "Contains" => Expression.Call(member, typeof(string).GetMethod("Contains", [typeof(string)])!, Expression.Constant(filter.Value)),
                "StartsWith" => Expression.Call(member, typeof(string).GetMethod("StartsWith", [typeof(string)])!, Expression.Constant(filter.Value)),
                "EndsWith" => Expression.Call(member, typeof(string).GetMethod("EndsWith", [typeof(string)])!, Expression.Constant(filter.Value)),
                _ => throw new InvalidOperationException($"Operator {filter.Operator} is not supported")
            };

            predicate = predicate == null ? condition : Expression.AndAlso(predicate, condition);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(predicate!, parameter);
        return query.Where(lambda);
    }


    private static IQueryable<TSource> GetSortedSource<TSource>(IQueryable<TSource> source, MethodInfo genericMethod, LambdaExpression lambda)
    {
        object ret = genericMethod?.Invoke(null, [source, lambda])
            ?? throw new ArgumentNullException(nameof(genericMethod));

        return (IQueryable<TSource>)ret;
    }

    private static Expression GetOrderByExpression<TSource>(string propertyName, out LambdaExpression lambda)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "posting");
        Expression orderByProperty = Expression.Property(parameter, propertyName);

        lambda = Expression.Lambda(orderByProperty, parameter);
        return orderByProperty;
    }

    private static void ValidateFieldsAreAllowed<T>(IEnumerable<string> requestedFields)
    {
        var sensitiveFields = typeof(T)
            .GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(MarkAsSensitiveAttribute), true).Any())
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var forbidden = requestedFields
            .Where(f => sensitiveFields.Contains(f))
            .ToList();

        if (forbidden.Count != 0)
            throw new UnauthorizedAccessException(
                $"Access denied to sensitive fields: {string.Join(", ", forbidden)}");
    }
    
}
