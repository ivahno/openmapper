using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Shared helpers for working with ITypeSymbol and IPropertySymbol.
/// </summary>
internal static class TypeSymbolHelper
{
    public static List<IPropertySymbol> GetAllPublicProperties(INamedTypeSymbol type)
    {
        var properties = new List<IPropertySymbol>();
        var current = type;

        while (current is not null)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && prop.DeclaredAccessibility == Accessibility.Public
                    && !prop.IsStatic
                    && !prop.IsIndexer
                    && prop.GetMethod is not null)
                {
                    // Avoid duplicates from overrides
                    if (!properties.Any(p => string.Equals(p.Name, prop.Name, StringComparison.Ordinal)))
                    {
                        properties.Add(prop);
                    }
                }
            }

            current = current.BaseType;
        }

        return properties;
    }

    public static string GetFullTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
    }

    public static bool HasAttribute(IPropertySymbol property, string attributeFullName)
    {
        return property.GetAttributes().Any(a =>
        {
            var attrName = GetFullTypeName(a.AttributeClass!);
            return string.Equals(attrName, attributeFullName, StringComparison.Ordinal);
        });
    }

    public static AttributeData? GetAttribute(IPropertySymbol property, string attributeFullName)
    {
        return property.GetAttributes().FirstOrDefault(a =>
        {
            if (a.AttributeClass is null) return false;
            var attrName = GetFullTypeName(a.AttributeClass);
            return string.Equals(attrName, attributeFullName, StringComparison.Ordinal);
        });
    }
}
