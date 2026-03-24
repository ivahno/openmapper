using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenAutoMapper.Generator.Helpers;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Data about a discovered Profile subclass, extracted during syntax/semantic analysis.
/// Must be equatable for incremental generator caching.
/// </summary>
internal sealed class ProfileInfo : IEquatable<ProfileInfo>
{
    public ProfileInfo(
        string className,
        string @namespace,
        EquatableArray<TypePairReference> typePairs,
        string filePath,
        int line)
        : this(className, @namespace, typePairs, filePath, line,
               new EquatableArray<string>(ImmutableArray<string>.Empty),
               new EquatableArray<string>(ImmutableArray<string>.Empty))
    {
    }

    public ProfileInfo(
        string className,
        string @namespace,
        EquatableArray<TypePairReference> typePairs,
        string filePath,
        int line,
        EquatableArray<string> prefixes,
        EquatableArray<string> postfixes)
        : this(className, @namespace, typePairs, filePath, line, prefixes, postfixes, false)
    {
    }

    public ProfileInfo(
        string className,
        string @namespace,
        EquatableArray<TypePairReference> typePairs,
        string filePath,
        int line,
        EquatableArray<string> prefixes,
        EquatableArray<string> postfixes,
        bool allowNullCollections)
    {
        ClassName = className;
        Namespace = @namespace;
        TypePairs = typePairs;
        FilePath = filePath;
        Line = line;
        Prefixes = prefixes;
        Postfixes = postfixes;
        AllowNullCollections = allowNullCollections;
    }

    /// <summary>The simple class name of the profile.</summary>
    public string ClassName { get; }

    /// <summary>The namespace the profile is declared in.</summary>
    public string Namespace { get; }

    /// <summary>The list of source/dest full type name pairs found in CreateMap calls.</summary>
    public EquatableArray<TypePairReference> TypePairs { get; }

    /// <summary>The file path where the profile was found (for diagnostics).</summary>
    public string FilePath { get; }

    /// <summary>The line number where the profile was found (for diagnostics).</summary>
    public int Line { get; }

    /// <summary>Recognized prefixes for property name matching (e.g., "Get", "Is").</summary>
    public EquatableArray<string> Prefixes { get; }

    /// <summary>Recognized postfixes for property name matching (e.g., "Dto").</summary>
    public EquatableArray<string> Postfixes { get; }

    /// <summary>Whether null source collections should map to null instead of empty.</summary>
    public bool AllowNullCollections { get; }

    public bool Equals(ProfileInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(ClassName, other.ClassName, StringComparison.Ordinal)
            && string.Equals(Namespace, other.Namespace, StringComparison.Ordinal)
            && TypePairs.Equals(other.TypePairs)
            && string.Equals(FilePath, other.FilePath, StringComparison.Ordinal)
            && Line == other.Line
            && Prefixes.Equals(other.Prefixes)
            && Postfixes.Equals(other.Postfixes)
            && AllowNullCollections == other.AllowNullCollections;
    }

    public override bool Equals(object? obj)
    {
        return obj is ProfileInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ClassName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Namespace);
            hash = hash * 31 + TypePairs.GetHashCode();
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(FilePath);
            hash = hash * 31 + Line;
            hash = hash * 31 + Prefixes.GetHashCode();
            hash = hash * 31 + Postfixes.GetHashCode();
            hash = hash * 31 + AllowNullCollections.GetHashCode();
            return hash;
        }
    }
}

/// <summary>
/// A reference to a source/destination type pair by fully qualified name.
/// </summary>
internal sealed class TypePairReference : IEquatable<TypePairReference>
{
    public TypePairReference(string sourceFullName, string destFullName)
        : this(sourceFullName, destFullName, new EquatableArray<MemberConfigReference>(ImmutableArray<MemberConfigReference>.Empty))
    {
    }

    public TypePairReference(string sourceFullName, string destFullName, EquatableArray<MemberConfigReference> memberConfigs)
        : this(sourceFullName, destFullName, memberConfigs,
               new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty),
               new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty),
               null)
    {
    }

    public TypePairReference(
        string sourceFullName,
        string destFullName,
        EquatableArray<MemberConfigReference> memberConfigs,
        EquatableArray<IncludedTypeReference> includedDerivedTypes,
        EquatableArray<IncludedTypeReference> includedBaseTypes,
        int? maxDepth)
        : this(sourceFullName, destFullName, memberConfigs, includedDerivedTypes, includedBaseTypes, maxDepth,
               false, null, null, null, null)
    {
    }

    public TypePairReference(
        string sourceFullName,
        string destFullName,
        EquatableArray<MemberConfigReference> memberConfigs,
        EquatableArray<IncludedTypeReference> includedDerivedTypes,
        EquatableArray<IncludedTypeReference> includedBaseTypes,
        int? maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression)
        : this(sourceFullName, destFullName, memberConfigs, includedDerivedTypes, includedBaseTypes,
               maxDepth, isProjection, beforeMapExpression, afterMapExpression,
               constructUsingExpression, convertUsingExpression,
               new EquatableArray<CtorParamConfigReference>(ImmutableArray<CtorParamConfigReference>.Empty))
    {
    }

    public TypePairReference(
        string sourceFullName,
        string destFullName,
        EquatableArray<MemberConfigReference> memberConfigs,
        EquatableArray<IncludedTypeReference> includedDerivedTypes,
        EquatableArray<IncludedTypeReference> includedBaseTypes,
        int? maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression,
        EquatableArray<CtorParamConfigReference> ctorParamConfigs)
        : this(sourceFullName, destFullName, memberConfigs, includedDerivedTypes, includedBaseTypes,
               maxDepth, isProjection, beforeMapExpression, afterMapExpression,
               constructUsingExpression, convertUsingExpression, ctorParamConfigs,
               null, new EquatableArray<string>(ImmutableArray<string>.Empty),
               null, false, new EquatableArray<string>(ImmutableArray<string>.Empty), null)
    {
    }

    public TypePairReference(
        string sourceFullName,
        string destFullName,
        EquatableArray<MemberConfigReference> memberConfigs,
        EquatableArray<IncludedTypeReference> includedDerivedTypes,
        EquatableArray<IncludedTypeReference> includedBaseTypes,
        int? maxDepth,
        bool isProjection,
        string? beforeMapExpression,
        string? afterMapExpression,
        string? constructUsingExpression,
        string? convertUsingExpression,
        EquatableArray<CtorParamConfigReference> ctorParamConfigs,
        MemberConfigReference? forAllMembersConfig,
        EquatableArray<string> includedMemberNames,
        int? enumMappingStrategy,
        bool isDeepClone,
        EquatableArray<string> additionalSourceTypes,
        string? mappingName)
    {
        SourceFullName = sourceFullName;
        DestFullName = destFullName;
        MemberConfigs = memberConfigs;
        IncludedDerivedTypes = includedDerivedTypes;
        IncludedBaseTypes = includedBaseTypes;
        MaxDepth = maxDepth;
        IsProjection = isProjection;
        BeforeMapExpression = beforeMapExpression;
        AfterMapExpression = afterMapExpression;
        ConstructUsingExpression = constructUsingExpression;
        ConvertUsingExpression = convertUsingExpression;
        CtorParamConfigs = ctorParamConfigs;
        ForAllMembersConfig = forAllMembersConfig;
        IncludedMemberNames = includedMemberNames;
        EnumMappingStrategy = enumMappingStrategy;
        IsDeepClone = isDeepClone;
        AdditionalSourceTypes = additionalSourceTypes;
        MappingName = mappingName;
    }

    public string SourceFullName { get; }
    public string DestFullName { get; }
    public EquatableArray<MemberConfigReference> MemberConfigs { get; }
    public EquatableArray<IncludedTypeReference> IncludedDerivedTypes { get; }
    public EquatableArray<IncludedTypeReference> IncludedBaseTypes { get; }
    public int? MaxDepth { get; }
    public bool IsProjection { get; }
    public string? BeforeMapExpression { get; }
    public string? AfterMapExpression { get; }
    public string? ConstructUsingExpression { get; }
    public string? ConvertUsingExpression { get; }
    public EquatableArray<CtorParamConfigReference> CtorParamConfigs { get; }
    public MemberConfigReference? ForAllMembersConfig { get; }
    public EquatableArray<string> IncludedMemberNames { get; }
    public int? EnumMappingStrategy { get; }
    public bool IsDeepClone { get; }
    public EquatableArray<string> AdditionalSourceTypes { get; }
    public string? MappingName { get; }

    public bool Equals(TypePairReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourceFullName, other.SourceFullName, StringComparison.Ordinal)
            && string.Equals(DestFullName, other.DestFullName, StringComparison.Ordinal)
            && MemberConfigs.Equals(other.MemberConfigs)
            && IncludedDerivedTypes.Equals(other.IncludedDerivedTypes)
            && IncludedBaseTypes.Equals(other.IncludedBaseTypes)
            && MaxDepth == other.MaxDepth
            && IsProjection == other.IsProjection
            && string.Equals(BeforeMapExpression, other.BeforeMapExpression, StringComparison.Ordinal)
            && string.Equals(AfterMapExpression, other.AfterMapExpression, StringComparison.Ordinal)
            && string.Equals(ConstructUsingExpression, other.ConstructUsingExpression, StringComparison.Ordinal)
            && string.Equals(ConvertUsingExpression, other.ConvertUsingExpression, StringComparison.Ordinal)
            && CtorParamConfigs.Equals(other.CtorParamConfigs)
            && Equals(ForAllMembersConfig, other.ForAllMembersConfig)
            && IncludedMemberNames.Equals(other.IncludedMemberNames)
            && EnumMappingStrategy == other.EnumMappingStrategy
            && IsDeepClone == other.IsDeepClone
            && AdditionalSourceTypes.Equals(other.AdditionalSourceTypes)
            && string.Equals(MappingName, other.MappingName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is TypePairReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = StringComparer.Ordinal.GetHashCode(SourceFullName) * 397;
            hash ^= StringComparer.Ordinal.GetHashCode(DestFullName);
            hash = hash * 31 + MemberConfigs.GetHashCode();
            hash = hash * 31 + IncludedDerivedTypes.GetHashCode();
            hash = hash * 31 + IncludedBaseTypes.GetHashCode();
            hash = hash * 31 + (MaxDepth ?? 0);
            hash = hash * 31 + IsProjection.GetHashCode();
            hash = hash * 31 + (BeforeMapExpression is not null ? StringComparer.Ordinal.GetHashCode(BeforeMapExpression) : 0);
            hash = hash * 31 + (AfterMapExpression is not null ? StringComparer.Ordinal.GetHashCode(AfterMapExpression) : 0);
            hash = hash * 31 + (ConstructUsingExpression is not null ? StringComparer.Ordinal.GetHashCode(ConstructUsingExpression) : 0);
            hash = hash * 31 + (ConvertUsingExpression is not null ? StringComparer.Ordinal.GetHashCode(ConvertUsingExpression) : 0);
            hash = hash * 31 + CtorParamConfigs.GetHashCode();
            hash = hash * 31 + (ForAllMembersConfig is not null ? ForAllMembersConfig.GetHashCode() : 0);
            hash = hash * 31 + IncludedMemberNames.GetHashCode();
            hash = hash * 31 + (EnumMappingStrategy ?? 0);
            hash = hash * 31 + IsDeepClone.GetHashCode();
            hash = hash * 31 + AdditionalSourceTypes.GetHashCode();
            hash = hash * 31 + (MappingName is not null ? StringComparer.Ordinal.GetHashCode(MappingName) : 0);
            return hash;
        }
    }
}
