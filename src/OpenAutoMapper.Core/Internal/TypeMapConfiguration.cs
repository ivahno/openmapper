#nullable enable

using System;
using System.Collections.Generic;

namespace OpenAutoMapper.Internal;

/// <summary>
/// Stores all configuration for a source-to-destination type mapping pair.
/// Reflection-based access to SourceType/DestinationType properties is only used
/// during configuration validation; actual mapping is performed by the source generator
/// without reflection.
/// </summary>
internal sealed class TypeMapConfiguration
{
    public TypeMapConfiguration(Type sourceType, Type destinationType, MemberList memberList, bool isProjection)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        MemberList = memberList;
        IsProjection = isProjection;
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
    /// The individual property mappings configured for this type pair.
    /// </summary>
    public List<PropertyMap> PropertyMaps { get; } = new();

    /// <summary>
    /// Path-based mappings (ForPath) for this type pair.
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
    /// Derived type pairs included in this mapping.
    /// </summary>
    public List<TypePair> IncludedDerivedTypes { get; } = new();

    /// <summary>
    /// Base type pairs included in this mapping.
    /// </summary>
    public List<TypePair> IncludedBaseTypes { get; } = new();

    /// <summary>
    /// Maximum depth for recursive mapping.
    /// </summary>
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Specifies which member list to validate.
    /// </summary>
    public MemberList MemberList { get; }

    /// <summary>
    /// Whether this configuration is for an expression-based projection.
    /// </summary>
    public bool IsProjection { get; }

    /// <summary>
    /// The reverse map configuration, if ReverseMap() was called.
    /// </summary>
    public TypeMapConfiguration? ReverseMapConfiguration { get; set; }

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

    /// <summary>
    /// Constructor parameter mappings: param name -> source member name.
    /// </summary>
    public Dictionary<string, string> CtorParamMappings { get; } = new();

    /// <summary>
    /// ForAllMembers default config applied to all members without explicit ForMember override.
    /// </summary>
    public PropertyMap? ForAllMembersConfig { get; set; }

    /// <summary>
    /// Member names included via IncludeMembers (flattening through navigation properties).
    /// </summary>
    public List<string> IncludedMemberNames { get; } = new();

    /// <summary>
    /// Whether deep cloning is enabled for this mapping.
    /// </summary>
    public bool IsDeepClone { get; set; }

    /// <summary>
    /// Additional source types registered via IncludeSource.
    /// </summary>
    public List<Type> AdditionalSourceTypes { get; } = new();

    /// <summary>
    /// Optional mapping name for named mappings.
    /// </summary>
    public string? MappingName { get; set; }
}
