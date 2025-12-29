using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PageSort.Core.Extensions;

/// <summary>
/// 
/// </summary>
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
