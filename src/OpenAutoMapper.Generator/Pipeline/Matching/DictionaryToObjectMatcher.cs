using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Detects when the source type is a string-keyed dictionary and creates DictionaryToObject
/// property matches by matching dictionary keys to destination property names.
/// </summary>
internal static class DictionaryToObjectMatcher
{
    /// <summary>
    /// Returns true if the source type is a string-keyed dictionary (Dictionary&lt;string, T&gt;).
    /// </summary>
    public static bool IsStringKeyedDictionary(INamedTypeSymbol sourceType, out ITypeSymbol? valueType)
    {
        valueType = null;
        if (!sourceType.IsGenericType || sourceType.TypeArguments.Length != 2)
            return false;

        var originalDef = sourceType.OriginalDefinition.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

        switch (originalDef)
        {
            case "System.Collections.Generic.Dictionary<TKey, TValue>":
            case "System.Collections.Generic.IDictionary<TKey, TValue>":
            case "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>":
                break;
            default:
                return false;
        }

        var keyType = sourceType.TypeArguments[0];
        if (keyType.SpecialType != SpecialType.System_String)
            return false;

        valueType = sourceType.TypeArguments[1];
        return true;
    }

    /// <summary>
    /// Creates PropertyMatchDescriptors for each destination property that can be read from
    /// the source dictionary by key name.
    /// </summary>
    public static List<PropertyMatchDescriptor> MatchDictionaryToObject(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        ITypeSymbol dictValueType)
    {
        var matches = new List<PropertyMatchDescriptor>();
        var destProperties = TypeSymbolHelper.GetAllPublicProperties(destType);
        var valueFullType = TypeSymbolHelper.GetFullTypeName(dictValueType);
        var sourceFullType = TypeSymbolHelper.GetFullTypeName(sourceType);

        foreach (var destProp in destProperties)
        {
            if (destProp.SetMethod is null || destProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            var isInitOnly = destProp.SetMethod.IsInitOnly;

            matches.Add(new PropertyMatchDescriptor(
                destProp.Name,
                valueFullType,
                destProp.Name,
                TypeSymbolHelper.GetFullTypeName(destProp.Type),
                ConversionKind.DictionaryToObject,
                null, null, CollectionKind.None, null, null, null,
                null, null, null, null, null, null,
                isInitOnly));
        }

        return matches;
    }
}
