#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Specifies which member list to validate for configuration correctness.
/// </summary>
public enum MemberList
{
    /// <summary>
    /// Validate that all destination members are mapped.
    /// </summary>
    Destination = 0,

    /// <summary>
    /// Validate that all source members are mapped.
    /// </summary>
    Source = 1,

    /// <summary>
    /// Skip member validation entirely.
    /// </summary>
    None = 2,
}
