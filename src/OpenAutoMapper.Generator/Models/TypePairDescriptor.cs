using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenAutoMapper.Generator.Helpers;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Fully analyzed type pair with matched properties, ready for code emission.
/// Must be equatable for incremental generator caching.
/// </summary>
internal sealed class TypePairDescriptor : IEquatable<TypePairDescriptor>
{
    public TypePairDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName,
        EquatableArray<PropertyMatchDescriptor> propertyMatches)
        : this(sourceFullName, sourceName, destFullName, destName, propertyMatches,
               new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty),
               false, 10)
    {
    }

    public TypePairDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName,
        EquatableArray<PropertyMatchDescriptor> propertyMatches,
        EquatableArray<IncludedTypeDescriptor> includedDerivedTypes,
        bool hasCyclicReference,
        int maxDepth)
        : this(sourceFullName, sourceName, destFullName, destName, propertyMatches,
               includedDerivedTypes, hasCyclicReference, maxDepth, false, null, null, null, null)
    {
    }

    public TypePairDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName,
        EquatableArray<PropertyMatchDescriptor> propertyMatches,
        EquatableArray<IncludedTypeDescriptor> includedDerivedTypes,
        bool hasCyclicReference,
        int maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression)
        : this(sourceFullName, sourceName, destFullName, destName, propertyMatches,
               includedDerivedTypes, hasCyclicReference, maxDepth, isProjection,
               beforeMapExpression, afterMapExpression, constructUsingExpression, convertUsingExpression,
               new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty))
    {
    }

    public TypePairDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName,
        EquatableArray<PropertyMatchDescriptor> propertyMatches,
        EquatableArray<IncludedTypeDescriptor> includedDerivedTypes,
        bool hasCyclicReference,
        int maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression,
        EquatableArray<ConstructorParamDescriptor> constructorParams)
        : this(sourceFullName, sourceName, destFullName, destName, propertyMatches,
               includedDerivedTypes, hasCyclicReference, maxDepth, isProjection,
               beforeMapExpression, afterMapExpression, constructUsingExpression, convertUsingExpression,
               constructorParams, null, false, false,
               new EquatableArray<string>(ImmutableArray<string>.Empty), null)
    {
    }

    public TypePairDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName,
        EquatableArray<PropertyMatchDescriptor> propertyMatches,
        EquatableArray<IncludedTypeDescriptor> includedDerivedTypes,
        bool hasCyclicReference,
        int maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression,
        EquatableArray<ConstructorParamDescriptor> constructorParams,
        int? enumMappingStrategy,
        bool allowNullCollections,
        bool isDeepClone,
        EquatableArray<string> additionalSourceTypes,
        string? mappingName)
    {
        SourceFullName = sourceFullName;
        SourceName = sourceName;
        DestFullName = destFullName;
        DestName = destName;
        PropertyMatches = propertyMatches;
        IncludedDerivedTypes = includedDerivedTypes;
        HasCyclicReference = hasCyclicReference;
        MaxDepth = maxDepth;
        IsProjection = isProjection;
        BeforeMapExpression = beforeMapExpression;
        AfterMapExpression = afterMapExpression;
        ConstructUsingExpression = constructUsingExpression;
        ConvertUsingExpression = convertUsingExpression;
        ConstructorParams = constructorParams;
        EnumMappingStrategy = enumMappingStrategy;
        AllowNullCollections = allowNullCollections;
        IsDeepClone = isDeepClone;
        AdditionalSourceTypes = additionalSourceTypes;
        MappingName = mappingName;
    }

    /// <summary>Fully qualified source type name (e.g., "MyApp.Models.OrderDto").</summary>
    public string SourceFullName { get; }

    /// <summary>Simple source type name (e.g., "OrderDto").</summary>
    public string SourceName { get; }

    /// <summary>Fully qualified destination type name.</summary>
    public string DestFullName { get; }

    /// <summary>Simple destination type name.</summary>
    public string DestName { get; }

    /// <summary>The matched property pairs for this type mapping.</summary>
    public EquatableArray<PropertyMatchDescriptor> PropertyMatches { get; }

    /// <summary>Derived type pairs included via Include&lt;S,D&gt;().</summary>
    public EquatableArray<IncludedTypeDescriptor> IncludedDerivedTypes { get; }

    /// <summary>Whether this type pair participates in a circular reference cycle.</summary>
    public bool HasCyclicReference { get; set; }

    /// <summary>Maximum depth for circular reference tracking (default 10).</summary>
    public int MaxDepth { get; }

    /// <summary>Whether this is a projection (CreateProjection) vs a mapping (CreateMap).</summary>
    public bool IsProjection { get; }

    /// <summary>BeforeMap lambda body text, if specified.</summary>
    public string? BeforeMapExpression { get; }

    /// <summary>AfterMap lambda body text, if specified.</summary>
    public string? AfterMapExpression { get; }

    /// <summary>ConstructUsing lambda body text, if specified.</summary>
    public string? ConstructUsingExpression { get; }

    /// <summary>ConvertUsing lambda body text, if specified.</summary>
    public string? ConvertUsingExpression { get; }

    /// <summary>Constructor parameters matched for code generation.</summary>
    public EquatableArray<ConstructorParamDescriptor> ConstructorParams { get; }

    /// <summary>Enum mapping strategy override (null = default ByName).</summary>
    public int? EnumMappingStrategy { get; }

    /// <summary>Whether null source collections should map to null instead of empty.</summary>
    public bool AllowNullCollections { get; }

    /// <summary>Whether deep cloning is enabled for this mapping.</summary>
    public bool IsDeepClone { get; }

    /// <summary>Additional source types registered via IncludeSource.</summary>
    public EquatableArray<string> AdditionalSourceTypes { get; }

    /// <summary>Optional mapping name for named mappings.</summary>
    public string? MappingName { get; }

    /// <summary>True if this mapping uses constructor mapping (has ctor params).</summary>
    public bool HasConstructorMapping => ConstructorParams.Length > 0;

    /// <summary>True if this mapping has derived types registered via Include.</summary>
    public bool IsPolymorphicBase => IncludedDerivedTypes.Length > 0;

    public bool Equals(TypePairDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourceFullName, other.SourceFullName, StringComparison.Ordinal)
            && string.Equals(SourceName, other.SourceName, StringComparison.Ordinal)
            && string.Equals(DestFullName, other.DestFullName, StringComparison.Ordinal)
            && string.Equals(DestName, other.DestName, StringComparison.Ordinal)
            && PropertyMatches.Equals(other.PropertyMatches)
            && IncludedDerivedTypes.Equals(other.IncludedDerivedTypes)
            && HasCyclicReference == other.HasCyclicReference
            && MaxDepth == other.MaxDepth
            && IsProjection == other.IsProjection
            && string.Equals(BeforeMapExpression, other.BeforeMapExpression, StringComparison.Ordinal)
            && string.Equals(AfterMapExpression, other.AfterMapExpression, StringComparison.Ordinal)
            && string.Equals(ConstructUsingExpression, other.ConstructUsingExpression, StringComparison.Ordinal)
            && string.Equals(ConvertUsingExpression, other.ConvertUsingExpression, StringComparison.Ordinal)
            && ConstructorParams.Equals(other.ConstructorParams)
            && EnumMappingStrategy == other.EnumMappingStrategy
            && AllowNullCollections == other.AllowNullCollections
            && IsDeepClone == other.IsDeepClone
            && AdditionalSourceTypes.Equals(other.AdditionalSourceTypes)
            && string.Equals(MappingName, other.MappingName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is TypePairDescriptor other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourceFullName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourceName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DestFullName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DestName);
            hash = hash * 31 + PropertyMatches.GetHashCode();
            hash = hash * 31 + IncludedDerivedTypes.GetHashCode();
            hash = hash * 31 + HasCyclicReference.GetHashCode();
            hash = hash * 31 + MaxDepth;
            hash = hash * 31 + IsProjection.GetHashCode();
            hash = hash * 31 + (BeforeMapExpression is not null ? StringComparer.Ordinal.GetHashCode(BeforeMapExpression) : 0);
            hash = hash * 31 + (AfterMapExpression is not null ? StringComparer.Ordinal.GetHashCode(AfterMapExpression) : 0);
            hash = hash * 31 + (ConstructUsingExpression is not null ? StringComparer.Ordinal.GetHashCode(ConstructUsingExpression) : 0);
            hash = hash * 31 + (ConvertUsingExpression is not null ? StringComparer.Ordinal.GetHashCode(ConvertUsingExpression) : 0);
            hash = hash * 31 + ConstructorParams.GetHashCode();
            hash = hash * 31 + (EnumMappingStrategy ?? 0);
            hash = hash * 31 + AllowNullCollections.GetHashCode();
            hash = hash * 31 + IsDeepClone.GetHashCode();
            hash = hash * 31 + AdditionalSourceTypes.GetHashCode();
            hash = hash * 31 + (MappingName is not null ? StringComparer.Ordinal.GetHashCode(MappingName) : 0);
            return hash;
        }
    }
}
