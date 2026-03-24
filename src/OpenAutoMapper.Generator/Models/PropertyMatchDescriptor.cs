using System;
using System.Collections.Immutable;
using OpenAutoMapper.Generator.Helpers;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Describes a matched pair of source and destination properties for code generation.
/// </summary>
internal sealed class PropertyMatchDescriptor : IEquatable<PropertyMatchDescriptor>
{
    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, null, null, CollectionKind.None, null, null, null, null, null)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, sourceElementType, destElementType, destCollectionKind, null, null, null, null, null)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind,
        EquatableArray<EnumMemberPair>? enumMembers,
        string? dictKeyType,
        string? dictValueType)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, sourceElementType, destElementType, destCollectionKind, enumMembers,
               dictKeyType, dictValueType, null, null)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind,
        EquatableArray<EnumMemberPair>? enumMembers,
        string? dictKeyType,
        string? dictValueType,
        string? conditionExpression,
        string? preConditionExpression)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, sourceElementType, destElementType, destCollectionKind, enumMembers,
               dictKeyType, dictValueType, conditionExpression, preConditionExpression, null, null)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind,
        EquatableArray<EnumMemberPair>? enumMembers,
        string? dictKeyType,
        string? dictValueType,
        string? conditionExpression,
        string? preConditionExpression,
        string? nullSubstituteExpression,
        string? valueResolverTypeName)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, sourceElementType, destElementType, destCollectionKind, enumMembers,
               dictKeyType, dictValueType, conditionExpression, preConditionExpression,
               nullSubstituteExpression, valueResolverTypeName, null, null)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind,
        EquatableArray<EnumMemberPair>? enumMembers,
        string? dictKeyType,
        string? dictValueType,
        string? conditionExpression,
        string? preConditionExpression,
        string? nullSubstituteExpression,
        string? valueResolverTypeName,
        string? destPathIntermediateTypes,
        string? memberValueResolverTypeName)
        : this(sourcePropertyName, sourcePropertyType, destPropertyName, destPropertyType,
               conversionKind, sourceElementType, destElementType, destCollectionKind, enumMembers,
               dictKeyType, dictValueType, conditionExpression, preConditionExpression,
               nullSubstituteExpression, valueResolverTypeName, destPathIntermediateTypes,
               memberValueResolverTypeName, false)
    {
    }

    public PropertyMatchDescriptor(
        string sourcePropertyName,
        string sourcePropertyType,
        string destPropertyName,
        string destPropertyType,
        ConversionKind conversionKind,
        string? sourceElementType,
        string? destElementType,
        CollectionKind destCollectionKind,
        EquatableArray<EnumMemberPair>? enumMembers,
        string? dictKeyType,
        string? dictValueType,
        string? conditionExpression,
        string? preConditionExpression,
        string? nullSubstituteExpression,
        string? valueResolverTypeName,
        string? destPathIntermediateTypes,
        string? memberValueResolverTypeName,
        bool isInitOnly)
    {
        SourcePropertyName = sourcePropertyName;
        SourcePropertyType = sourcePropertyType;
        DestPropertyName = destPropertyName;
        DestPropertyType = destPropertyType;
        ConversionKind = conversionKind;
        SourceElementType = sourceElementType;
        DestElementType = destElementType;
        DestCollectionKind = destCollectionKind;
        EnumMembers = enumMembers;
        DictKeyType = dictKeyType;
        DictValueType = dictValueType;
        ConditionExpression = conditionExpression;
        PreConditionExpression = preConditionExpression;
        NullSubstituteExpression = nullSubstituteExpression;
        ValueResolverTypeName = valueResolverTypeName;
        DestPathIntermediateTypes = destPathIntermediateTypes;
        MemberValueResolverTypeName = memberValueResolverTypeName;
        IsInitOnly = isInitOnly;
    }

    public string SourcePropertyName { get; }
    public string SourcePropertyType { get; }
    public string DestPropertyName { get; }
    public string DestPropertyType { get; }
    public ConversionKind ConversionKind { get; }
    public string? SourceElementType { get; }
    public string? DestElementType { get; }
    public CollectionKind DestCollectionKind { get; }
    public EquatableArray<EnumMemberPair>? EnumMembers { get; }
    public string? DictKeyType { get; }
    public string? DictValueType { get; }
    public string? ConditionExpression { get; }
    public string? PreConditionExpression { get; }
    public string? NullSubstituteExpression { get; }
    public string? ValueResolverTypeName { get; }

    /// <summary>Pipe-delimited fully qualified type names for intermediate path segments in ForPath.</summary>
    public string? DestPathIntermediateTypes { get; }

    /// <summary>Fully qualified member value resolver type name from MapFrom&lt;TResolver, TSourceMember&gt;().</summary>
    public string? MemberValueResolverTypeName { get; }

    /// <summary>Whether the destination property has an init-only setter.</summary>
    public bool IsInitOnly { get; }

    public bool Equals(PropertyMatchDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourcePropertyName, other.SourcePropertyName, StringComparison.Ordinal)
            && string.Equals(SourcePropertyType, other.SourcePropertyType, StringComparison.Ordinal)
            && string.Equals(DestPropertyName, other.DestPropertyName, StringComparison.Ordinal)
            && string.Equals(DestPropertyType, other.DestPropertyType, StringComparison.Ordinal)
            && ConversionKind == other.ConversionKind
            && string.Equals(SourceElementType, other.SourceElementType, StringComparison.Ordinal)
            && string.Equals(DestElementType, other.DestElementType, StringComparison.Ordinal)
            && DestCollectionKind == other.DestCollectionKind
            && Nullable.Equals(EnumMembers, other.EnumMembers)
            && string.Equals(DictKeyType, other.DictKeyType, StringComparison.Ordinal)
            && string.Equals(DictValueType, other.DictValueType, StringComparison.Ordinal)
            && string.Equals(ConditionExpression, other.ConditionExpression, StringComparison.Ordinal)
            && string.Equals(PreConditionExpression, other.PreConditionExpression, StringComparison.Ordinal)
            && string.Equals(NullSubstituteExpression, other.NullSubstituteExpression, StringComparison.Ordinal)
            && string.Equals(ValueResolverTypeName, other.ValueResolverTypeName, StringComparison.Ordinal)
            && string.Equals(DestPathIntermediateTypes, other.DestPathIntermediateTypes, StringComparison.Ordinal)
            && string.Equals(MemberValueResolverTypeName, other.MemberValueResolverTypeName, StringComparison.Ordinal)
            && IsInitOnly == other.IsInitOnly;
    }

    public override bool Equals(object? obj)
    {
        return obj is PropertyMatchDescriptor other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePropertyName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePropertyType);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DestPropertyName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DestPropertyType);
            hash = hash * 31 + (int)ConversionKind;
            hash = hash * 31 + (SourceElementType is not null ? StringComparer.Ordinal.GetHashCode(SourceElementType) : 0);
            hash = hash * 31 + (DestElementType is not null ? StringComparer.Ordinal.GetHashCode(DestElementType) : 0);
            hash = hash * 31 + (int)DestCollectionKind;
            hash = hash * 31 + (EnumMembers?.GetHashCode() ?? 0);
            hash = hash * 31 + (DictKeyType is not null ? StringComparer.Ordinal.GetHashCode(DictKeyType) : 0);
            hash = hash * 31 + (DictValueType is not null ? StringComparer.Ordinal.GetHashCode(DictValueType) : 0);
            hash = hash * 31 + (ConditionExpression is not null ? StringComparer.Ordinal.GetHashCode(ConditionExpression) : 0);
            hash = hash * 31 + (PreConditionExpression is not null ? StringComparer.Ordinal.GetHashCode(PreConditionExpression) : 0);
            hash = hash * 31 + (NullSubstituteExpression is not null ? StringComparer.Ordinal.GetHashCode(NullSubstituteExpression) : 0);
            hash = hash * 31 + (ValueResolverTypeName is not null ? StringComparer.Ordinal.GetHashCode(ValueResolverTypeName) : 0);
            hash = hash * 31 + (DestPathIntermediateTypes is not null ? StringComparer.Ordinal.GetHashCode(DestPathIntermediateTypes) : 0);
            hash = hash * 31 + (MemberValueResolverTypeName is not null ? StringComparer.Ordinal.GetHashCode(MemberValueResolverTypeName) : 0);
            hash = hash * 31 + IsInitOnly.GetHashCode();
            return hash;
        }
    }
}
