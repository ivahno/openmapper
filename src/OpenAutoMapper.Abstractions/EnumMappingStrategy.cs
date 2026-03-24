#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Specifies the strategy used to map between enum types.
/// </summary>
public enum EnumMappingStrategy
{
    /// <summary>Map enum members by matching name (case-sensitive). This is the default.</summary>
    ByName = 0,

    /// <summary>Map enum members by their underlying integer value.</summary>
    ByValue = 1,

    /// <summary>Map enum members by name using case-insensitive comparison.</summary>
    ByNameCaseInsensitive = 2,
}
