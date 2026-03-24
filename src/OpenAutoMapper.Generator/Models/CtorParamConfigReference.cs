using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Represents a ForCtorParam configuration extracted from the fluent chain.
/// </summary>
internal sealed class CtorParamConfigReference : IEquatable<CtorParamConfigReference>
{
    public CtorParamConfigReference(string paramName, string? sourceMemberName)
    {
        ParamName = paramName;
        SourceMemberName = sourceMemberName;
    }

    /// <summary>The constructor parameter name.</summary>
    public string ParamName { get; }

    /// <summary>The source member name to map from (if explicitly configured).</summary>
    public string? SourceMemberName { get; }

    public bool Equals(CtorParamConfigReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(ParamName, other.ParamName, StringComparison.Ordinal)
            && string.Equals(SourceMemberName, other.SourceMemberName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is CtorParamConfigReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ParamName);
            hash = hash * 31 + (SourceMemberName is not null ? StringComparer.Ordinal.GetHashCode(SourceMemberName) : 0);
            return hash;
        }
    }
}
