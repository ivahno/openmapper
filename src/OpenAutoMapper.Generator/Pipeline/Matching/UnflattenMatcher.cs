using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Matches properties by unflattening: dest has a complex property (e.g., Address) and source has
/// properties named ComplexPropName + SubPropName (e.g., AddressCity, AddressState).
/// Returns dotted-path PropertyMatchDescriptors for each matched sub-property.
/// </summary>
internal static class UnflattenMatcher
{
    public static List<PropertyMatchDescriptor>? TryUnflattenMatch(
        Compilation compilation,
        List<IPropertySymbol> sourceProperties,
        IPropertySymbol destProp)
    {
        // Only unflatten complex types (class/struct with sub-properties)
        if (destProp.Type is not INamedTypeSymbol destPropType)
            return null;

        if (destPropType.TypeKind != TypeKind.Class && destPropType.TypeKind != TypeKind.Struct)
            return null;

        // Skip primitive/system types
        if (destPropType.SpecialType != SpecialType.None)
            return null;

        var subProperties = TypeSymbolHelper.GetAllPublicProperties(destPropType);
        if (subProperties.Count == 0)
            return null;

        var results = new List<PropertyMatchDescriptor>();

        foreach (var subProp in subProperties)
        {
            if (subProp.SetMethod is null || subProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Check if source has a property named destPropName + subPropName
            var flatName = destProp.Name + subProp.Name;
            var sourceProp = sourceProperties.FirstOrDefault(
                sp => string.Equals(sp.Name, flatName, StringComparison.OrdinalIgnoreCase));

            if (sourceProp is null)
                continue;

            var convKind = ConversionResolver.DetermineConversion(compilation, sourceProp.Type, subProp.Type);
            var destPath = destProp.Name + "." + subProp.Name;
            var intermediateType = TypeSymbolHelper.GetFullTypeName(destPropType);

            results.Add(new PropertyMatchDescriptor(
                sourceProp.Name,
                TypeSymbolHelper.GetFullTypeName(sourceProp.Type),
                destPath,
                TypeSymbolHelper.GetFullTypeName(subProp.Type),
                convKind,
                null, null, CollectionKind.None, null, null, null,
                null, null, null, null,
                intermediateType,
                null));
        }

        return results.Count > 0 ? results : null;
    }
}
