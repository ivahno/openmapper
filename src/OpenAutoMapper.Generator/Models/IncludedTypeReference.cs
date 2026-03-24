using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Represents one Include&lt;S,D&gt;() or IncludeBase&lt;S,D&gt;() entry from fluent chain.
/// Stored as unresolved fully qualified names.
/// </summary>
internal sealed class IncludedTypeReference : IEquatable<IncludedTypeReference>
{
    public IncludedTypeReference(string sourceFullName, string destFullName)
    {
        SourceFullName = sourceFullName;
        DestFullName = destFullName;
    }

    public string SourceFullName { get; }
    public string DestFullName { get; }

    public bool Equals(IncludedTypeReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourceFullName, other.SourceFullName, StringComparison.Ordinal)
            && string.Equals(DestFullName, other.DestFullName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is IncludedTypeReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return StringComparer.Ordinal.GetHashCode(SourceFullName) * 397
                ^ StringComparer.Ordinal.GetHashCode(DestFullName);
        }
    }
}
