using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Analyzed version of an included type pair with both full and simple names.
/// </summary>
internal sealed class IncludedTypeDescriptor : IEquatable<IncludedTypeDescriptor>
{
    public IncludedTypeDescriptor(
        string sourceFullName,
        string sourceName,
        string destFullName,
        string destName)
    {
        SourceFullName = sourceFullName;
        SourceName = sourceName;
        DestFullName = destFullName;
        DestName = destName;
    }

    public string SourceFullName { get; }
    public string SourceName { get; }
    public string DestFullName { get; }
    public string DestName { get; }

    public bool Equals(IncludedTypeDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(SourceFullName, other.SourceFullName, StringComparison.Ordinal)
            && string.Equals(SourceName, other.SourceName, StringComparison.Ordinal)
            && string.Equals(DestFullName, other.DestFullName, StringComparison.Ordinal)
            && string.Equals(DestName, other.DestName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is IncludedTypeDescriptor other && Equals(other);
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
            return hash;
        }
    }
}
