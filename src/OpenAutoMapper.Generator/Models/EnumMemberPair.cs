using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Represents a matched pair of enum member names for compile-time switch generation.
/// </summary>
internal sealed class EnumMemberPair : IEquatable<EnumMemberPair>
{
    public EnumMemberPair(string sourceMemberName, string destMemberName)
    {
        SourceMemberName = sourceMemberName;
        DestMemberName = destMemberName;
    }

    public string SourceMemberName { get; }
    public string DestMemberName { get; }

    public bool Equals(EnumMemberPair? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourceMemberName, other.SourceMemberName, StringComparison.Ordinal)
            && string.Equals(DestMemberName, other.DestMemberName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is EnumMemberPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return StringComparer.Ordinal.GetHashCode(SourceMemberName) * 397
                ^ StringComparer.Ordinal.GetHashCode(DestMemberName);
        }
    }
}
