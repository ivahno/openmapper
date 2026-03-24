#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Specifies the source member name to map from for this destination property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MapFromAttribute : Attribute
{
    public MapFromAttribute(string sourceMemberName)
    {
        SourceMemberName = sourceMemberName ?? throw new ArgumentNullException(nameof(sourceMemberName));
    }

    /// <summary>
    /// The name of the source member to map from.
    /// </summary>
    public string SourceMemberName { get; }
}
