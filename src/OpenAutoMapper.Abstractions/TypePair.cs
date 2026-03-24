#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Represents a source-destination type pair used as a mapping key.
/// </summary>
public readonly struct TypePair : IEquatable<TypePair>
{
    public TypePair(Type sourceType, Type destinationType)
    {
        SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
        DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
    }

    public Type SourceType { get; }

    public Type DestinationType { get; }

    public bool Equals(TypePair other)
    {
        return SourceType == other.SourceType && DestinationType == other.DestinationType;
    }

    public override bool Equals(object? obj)
    {
        return obj is TypePair other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (SourceType.GetHashCode() * 397) ^ DestinationType.GetHashCode();
        }
    }

    public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);

    public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);

    public override string ToString() => $"{SourceType.Name} -> {DestinationType.Name}";
}
