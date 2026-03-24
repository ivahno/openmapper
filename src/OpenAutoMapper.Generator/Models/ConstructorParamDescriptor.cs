using System;

namespace OpenAutoMapper.Generator.Models;

/// <summary>
/// Describes a matched constructor parameter for code generation.
/// </summary>
internal sealed class ConstructorParamDescriptor : IEquatable<ConstructorParamDescriptor>
{
    public ConstructorParamDescriptor(
        string paramName,
        string paramType,
        string sourcePropertyName,
        string sourcePropertyType,
        ConversionKind conversionKind)
    {
        ParamName = paramName;
        ParamType = paramType;
        SourcePropertyName = sourcePropertyName;
        SourcePropertyType = sourcePropertyType;
        ConversionKind = conversionKind;
    }

    /// <summary>The constructor parameter name.</summary>
    public string ParamName { get; }

    /// <summary>Fully qualified constructor parameter type.</summary>
    public string ParamType { get; }

    /// <summary>The source property name that maps to this parameter.</summary>
    public string SourcePropertyName { get; }

    /// <summary>Fully qualified source property type.</summary>
    public string SourcePropertyType { get; }

    /// <summary>The conversion kind from source to parameter type.</summary>
    public ConversionKind ConversionKind { get; }

    public bool Equals(ConstructorParamDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(ParamName, other.ParamName, StringComparison.Ordinal)
            && string.Equals(ParamType, other.ParamType, StringComparison.Ordinal)
            && string.Equals(SourcePropertyName, other.SourcePropertyName, StringComparison.Ordinal)
            && string.Equals(SourcePropertyType, other.SourcePropertyType, StringComparison.Ordinal)
            && ConversionKind == other.ConversionKind;
    }

    public override bool Equals(object? obj)
    {
        return obj is ConstructorParamDescriptor other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ParamName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(ParamType);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePropertyName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePropertyType);
            hash = hash * 31 + (int)ConversionKind;
            return hash;
        }
    }
}
