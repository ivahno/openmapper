#nullable enable

using System;
using System.Collections.Generic;

namespace OpenAutoMapper.Internal;

/// <summary>
/// A resolved type map with all property maps resolved and ready for execution.
/// </summary>
internal sealed class TypeMap
{
    public TypeMap(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        TypePair = new TypePair(sourceType, destinationType);
    }

    /// <summary>
    /// The source type for this mapping.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// The destination type for this mapping.
    /// </summary>
    public Type DestinationType { get; }

    /// <summary>
    /// The type pair key for this mapping.
    /// </summary>
    public TypePair TypePair { get; }

    /// <summary>
    /// The resolved property maps for this type mapping.
    /// </summary>
    public List<PropertyMap> PropertyMaps { get; } = new();

    /// <summary>
    /// The resolved path maps for this type mapping.
    /// </summary>
    public List<PathMap> PathMaps { get; } = new();

    /// <summary>
    /// Custom constructor delegate for creating destination instances.
    /// </summary>
    public object? ConstructUsing { get; set; }

    /// <summary>
    /// Action to execute before mapping.
    /// </summary>
    public object? BeforeMap { get; set; }

    /// <summary>
    /// Action to execute after mapping.
    /// </summary>
    public object? AfterMap { get; set; }

    /// <summary>
    /// Maximum depth for recursive mapping.
    /// </summary>
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Specifies which member list to validate.
    /// </summary>
    public MemberList MemberList { get; set; }

    /// <summary>
    /// Whether this type map is for an expression-based projection.
    /// </summary>
    public bool IsProjection { get; set; }

    /// <summary>
    /// Custom type converter for this mapping.
    /// </summary>
    public object? ConvertUsing { get; set; }

    /// <summary>
    /// Mapping-level condition delegate.
    /// </summary>
    public object? Condition { get; set; }

    /// <summary>
    /// Mapping-level pre-condition delegate.
    /// </summary>
    public object? PreCondition { get; set; }

    /// <summary>
    /// Null substitute value for the entire mapping.
    /// </summary>
    public object? NullSubstitute { get; set; }
}
