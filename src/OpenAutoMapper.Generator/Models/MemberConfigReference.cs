using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Represents one fluent config entry extracted from .ForMember() / .Ignore() / .MapFrom().
/// </summary>
internal sealed class MemberConfigReference : IEquatable<MemberConfigReference>
{
    public MemberConfigReference(string destMemberName, string? sourceMemberName, bool isIgnored)
        : this(destMemberName, sourceMemberName, isIgnored, null, null, null, null)
    {
    }

    public MemberConfigReference(
        string destMemberName,
        string? sourceMemberName,
        bool isIgnored,
        string? conditionExpression,
        string? preConditionExpression)
        : this(destMemberName, sourceMemberName, isIgnored, conditionExpression, preConditionExpression, null, null)
    {
    }

    public MemberConfigReference(
        string destMemberName,
        string? sourceMemberName,
        bool isIgnored,
        string? conditionExpression,
        string? preConditionExpression,
        string? nullSubstituteExpression,
        string? valueResolverTypeName)
        : this(destMemberName, sourceMemberName, isIgnored, conditionExpression,
               preConditionExpression, nullSubstituteExpression, valueResolverTypeName, null)
    {
    }

    public MemberConfigReference(
        string destMemberName,
        string? sourceMemberName,
        bool isIgnored,
        string? conditionExpression,
        string? preConditionExpression,
        string? nullSubstituteExpression,
        string? valueResolverTypeName,
        string? memberValueResolverTypeName)
    {
        DestMemberName = destMemberName;
        SourceMemberName = sourceMemberName;
        IsIgnored = isIgnored;
        ConditionExpression = conditionExpression;
        PreConditionExpression = preConditionExpression;
        NullSubstituteExpression = nullSubstituteExpression;
        ValueResolverTypeName = valueResolverTypeName;
        MemberValueResolverTypeName = memberValueResolverTypeName;
    }

    public string DestMemberName { get; }
    public string? SourceMemberName { get; }
    public bool IsIgnored { get; }
    public string? ConditionExpression { get; }
    public string? PreConditionExpression { get; }

    /// <summary>NullSubstitute value expression text.</summary>
    public string? NullSubstituteExpression { get; }

    /// <summary>Fully qualified value resolver type name from MapFrom&lt;TResolver&gt;().</summary>
    public string? ValueResolverTypeName { get; }

    /// <summary>Fully qualified member value resolver type name from MapFrom&lt;TResolver, TSourceMember&gt;().</summary>
    public string? MemberValueResolverTypeName { get; }

    public bool Equals(MemberConfigReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(DestMemberName, other.DestMemberName, StringComparison.Ordinal)
            && string.Equals(SourceMemberName, other.SourceMemberName, StringComparison.Ordinal)
            && IsIgnored == other.IsIgnored
            && string.Equals(ConditionExpression, other.ConditionExpression, StringComparison.Ordinal)
            && string.Equals(PreConditionExpression, other.PreConditionExpression, StringComparison.Ordinal)
            && string.Equals(NullSubstituteExpression, other.NullSubstituteExpression, StringComparison.Ordinal)
            && string.Equals(ValueResolverTypeName, other.ValueResolverTypeName, StringComparison.Ordinal)
            && string.Equals(MemberValueResolverTypeName, other.MemberValueResolverTypeName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is MemberConfigReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DestMemberName);
            hash = hash * 31 + (SourceMemberName is not null ? StringComparer.Ordinal.GetHashCode(SourceMemberName) : 0);
            hash = hash * 31 + IsIgnored.GetHashCode();
            hash = hash * 31 + (ConditionExpression is not null ? StringComparer.Ordinal.GetHashCode(ConditionExpression) : 0);
            hash = hash * 31 + (PreConditionExpression is not null ? StringComparer.Ordinal.GetHashCode(PreConditionExpression) : 0);
            hash = hash * 31 + (NullSubstituteExpression is not null ? StringComparer.Ordinal.GetHashCode(NullSubstituteExpression) : 0);
            hash = hash * 31 + (ValueResolverTypeName is not null ? StringComparer.Ordinal.GetHashCode(ValueResolverTypeName) : 0);
            hash = hash * 31 + (MemberValueResolverTypeName is not null ? StringComparer.Ordinal.GetHashCode(MemberValueResolverTypeName) : 0);
            return hash;
        }
    }
}
