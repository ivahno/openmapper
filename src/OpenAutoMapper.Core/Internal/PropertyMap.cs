#nullable enable

using System;

namespace OpenAutoMapper.Internal;

/// <summary>
/// Represents an individual property mapping between source and destination members.
/// </summary>
internal sealed class PropertyMap
{
    /// <summary>
    /// The name of the source member, if explicitly configured.
    /// </summary>
    public string? SourceMemberName { get; set; }

    /// <summary>
    /// The name of the destination member.
    /// </summary>
    public string? DestinationMemberName { get; set; }

    /// <summary>
    /// Whether this destination member should be ignored during mapping.
    /// </summary>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// Custom mapping expression (lambda) for this property.
    /// </summary>
    public object? CustomMapExpression { get; set; }

    /// <summary>
    /// Condition delegate that must return true for this property to be mapped.
    /// </summary>
    public object? Condition { get; set; }

    /// <summary>
    /// Pre-condition delegate evaluated against the source before mapping this property.
    /// </summary>
    public object? PreCondition { get; set; }

    /// <summary>
    /// Value to use when the source member is null.
    /// </summary>
    public object? NullSubstitute { get; set; }

    /// <summary>
    /// The type of value resolver to use for this property.
    /// </summary>
    public Type? ValueResolverType { get; set; }

    /// <summary>
    /// The type of member value resolver to use for this property.
    /// </summary>
    public Type? MemberValueResolverType { get; set; }

    /// <summary>
    /// Whether to use the existing destination value instead of creating a new one.
    /// </summary>
    public bool UseDestinationValue { get; set; }
}
