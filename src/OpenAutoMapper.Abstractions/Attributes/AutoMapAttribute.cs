#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Marks a class for automatic mapping from the specified source type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoMapAttribute : Attribute
{
    public AutoMapAttribute(Type sourceType)
    {
        SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
    }

    /// <summary>
    /// The source type to map from.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Specifies which member list to validate. Defaults to <see cref="MemberList.Destination"/>.
    /// </summary>
    public MemberList MemberList { get; set; } = MemberList.Destination;

    /// <summary>
    /// When true, a reverse mapping (destination to source) is also created.
    /// </summary>
    public bool ReverseMap { get; set; }
}
