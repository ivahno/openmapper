#nullable enable

using System;

namespace OpenAutoMapper;

/// <summary>
/// Marks a property or field to be ignored during mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreMapAttribute : Attribute
{
}
