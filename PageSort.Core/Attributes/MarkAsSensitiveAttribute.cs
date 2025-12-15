using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageSort.Core.Attributes;

/// <summary>
/// Used to mark properties as sensitive so that they can be excluded from dynamic field selection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class MarkAsSensitiveAttribute : Attribute
{

}