#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Specifies a custom value resolver type for this destination property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValueResolverAttribute : Attribute
{
    public ValueResolverAttribute(Type resolverType)
    {
        ResolverType = resolverType ?? throw new ArgumentNullException(nameof(resolverType));
    }

    /// <summary>
    /// The type of the value resolver to use.
    /// </summary>
    public Type ResolverType { get; }
}
