using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenAutoMapper.Generator.Helpers;

/// <summary>
/// An immutable array wrapper that provides structural equality semantics,
/// required for correct incremental generator caching.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array)
    {
        _array = array;
    }

    public EquatableArray(IEnumerable<T> items)
    {
        _array = items.ToImmutableArray();
    }

    public ImmutableArray<T> AsImmutableArray()
    {
        return _array.IsDefault ? ImmutableArray<T>.Empty : _array;
    }

    public int Length => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];

    public bool Equals(EquatableArray<T> other)
    {
        var self = AsImmutableArray();
        var otherArray = other.AsImmutableArray();

        if (self.Length != otherArray.Length)
            return false;

        for (int i = 0; i < self.Length; i++)
        {
            if (!self[i].Equals(otherArray[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var arr = AsImmutableArray();
        unchecked
        {
            int hash = 17;
            foreach (var item in arr)
            {
                hash = hash * 31 + item.GetHashCode();
            }
            return hash;
        }
    }

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public ImmutableArray<T>.Enumerator GetEnumerator() => AsImmutableArray().GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        var arr = AsImmutableArray();
        foreach (var item in arr)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
}
