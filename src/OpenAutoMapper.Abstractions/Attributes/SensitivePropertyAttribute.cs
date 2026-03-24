#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Marks a property as sensitive, preventing accidental mapping.
/// The property must be explicitly configured to be included in a mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SensitivePropertyAttribute : Attribute
{
}
