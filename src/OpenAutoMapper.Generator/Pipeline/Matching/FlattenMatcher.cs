using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Matches properties by flattening: dest "AddressCity" → source "Address.City".
/// </summary>
internal static class FlattenMatcher
{
    public static (string accessPath, ITypeSymbol leafType)? TryFlattenMatch(
        List<IPropertySymbol> sourceProperties,
        string destPropertyName,
        INamedTypeSymbol sourceType)
    {
        // Try to decompose the destination property name into nested source access.
        // E.g., "AddressCity" -> source has "Address" property with "City" sub-property.
        foreach (var sourceProp in sourceProperties)
        {
            if (destPropertyName.StartsWith(sourceProp.Name, StringComparison.OrdinalIgnoreCase)
                && destPropertyName.Length > sourceProp.Name.Length)
            {
                var remainder = destPropertyName.Substring(sourceProp.Name.Length);
                var innerType = sourceProp.Type as INamedTypeSymbol;
                if (innerType is null)
                    continue;

                var innerProperties = TypeSymbolHelper.GetAllPublicProperties(innerType);
                var innerMatch = innerProperties.FirstOrDefault(
                    ip => string.Equals(ip.Name, remainder, StringComparison.OrdinalIgnoreCase));

                if (innerMatch is not null)
                {
                    return ($"{sourceProp.Name}.{innerMatch.Name}", innerMatch.Type);
                }
            }
        }

        return null;
    }
}
