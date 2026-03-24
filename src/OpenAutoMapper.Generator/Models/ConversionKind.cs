namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Describes the kind of conversion needed between a source and destination property.
/// </summary>
internal enum ConversionKind
{
    /// <summary>Direct assignment, types are identical.</summary>
    Direct,

    /// <summary>Implicit cast exists between the types.</summary>
    ImplicitCast,

    /// <summary>Explicit cast is required.</summary>
    ExplicitCast,

    /// <summary>Convert to string via ToString().</summary>
    ToString,

    /// <summary>Parse from string via a static Parse method.</summary>
    Parse,

    /// <summary>Enum mapping by name.</summary>
    EnumByName,

    /// <summary>Enum mapping by underlying integer value.</summary>
    EnumByValue,

    /// <summary>Nested object mapping (another mapped type pair).</summary>
    Nested,

    /// <summary>Unwrap Nullable&lt;T&gt; to T (e.g., int? → int).</summary>
    NullableUnwrap,

    /// <summary>Wrap T into Nullable&lt;T&gt; (e.g., int → int?).</summary>
    NullableWrap,

    /// <summary>Unwrap Nullable&lt;T&gt;, convert, then wrap (e.g., int? → long?).</summary>
    NullableConvert,

    /// <summary>Collection mapping (e.g., List&lt;T&gt; → List&lt;U&gt;).</summary>
    Collection,

    /// <summary>Dictionary mapping (e.g., Dictionary&lt;K,V&gt; → Dictionary&lt;K,V&gt;).</summary>
    Dictionary,

    /// <summary>Enum flags mapping by underlying integer value cast.</summary>
    EnumFlagsByValue,

    /// <summary>Dictionary-to-object mapping (string-keyed dictionary to POCO).</summary>
    DictionaryToObject,

    /// <summary>Enum mapping by name with case-insensitive comparison.</summary>
    EnumByNameCaseInsensitive,
}
