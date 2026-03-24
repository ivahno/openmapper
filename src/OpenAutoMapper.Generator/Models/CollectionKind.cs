namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Describes the materialization target for collection mappings.
/// </summary>
internal enum CollectionKind
{
    /// <summary>Not a collection.</summary>
    None,

    /// <summary>Materialize as T[].</summary>
    Array,

    /// <summary>Materialize as List&lt;T&gt;.</summary>
    List,

    /// <summary>Materialize as HashSet&lt;T&gt;.</summary>
    HashSet,
}
