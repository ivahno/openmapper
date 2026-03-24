#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Alias for <see cref="IgnoreMapAttribute"/>. Marks a property or field to be ignored during mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IgnoreAttribute : IgnoreMapAttribute
{
}
