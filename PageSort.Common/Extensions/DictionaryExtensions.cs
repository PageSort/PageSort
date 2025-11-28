using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PageSort.Common.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Converts a collection of dictionaries into a flat IEnumerable of key-value objects.
    /// Each dictionary entry becomes an object with Key and Value properties.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, object?>> ToEnumerable(this IQueryable<Dictionary<string, object>>? source)
    {
        if (source == null) yield break;

        foreach (var dict in source)
        {
            foreach (var kvp in dict)
            {
                yield return kvp!;
            }
        }
    }
    /// <summary>
    /// Maps a collection of dictionaries to a strongly-typed IEnumerable of TDestination.
    /// Only properties that match dictionary keys (case-insensitive) will be set.
    /// </summary>
    public static IEnumerable<TDestination> MapTo<TDestination>(this IEnumerable<Dictionary<string, object>> source) 
        where TDestination : new()
    {
        if (source == null) yield break;

        var properties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var dict in source)
        {
            var obj = new TDestination();

            foreach (var kvp in dict)
            {
                if (properties.TryGetValue(kvp.Key, out var prop))
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = kvp.Value == null ? null : Convert.ChangeType(kvp.Value, targetType);
                        prop.SetValue(obj, safeValue);
                    }
                    catch
                    {

                    }
                }
            }

            yield return obj;
        }
    }

    /// <summary>
    /// Converts a collection of dictionaries to a collection of dynamic objects (ExpandoObject),
    /// keeping only the fields present in each dictionary.
    /// </summary>
    public static IEnumerable<ExpandoObject> ToDynamicObjects(this IEnumerable<Dictionary<string, object>> source)
    {
        if (source == null) yield break;

        foreach (var dict in source)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)expando;

            foreach (var kvp in dict)
            {
                expandoDict[kvp.Key] = kvp.Value;
            }

            yield return expando;
        }
    }
}
