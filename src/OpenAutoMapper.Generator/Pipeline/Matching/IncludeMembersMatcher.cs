using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Matches unmatched destination properties by searching through IncludeMembers navigation properties.
/// For example, if IncludeMembers(s => s.Address) is specified, and dest has "City",
/// this matcher will try source.Address.City.
/// </summary>
internal static class IncludeMembersMatcher
{
    public static PropertyMatchDescriptor? TryMatch(
        Compilation compilation,
        IPropertySymbol destProp,
        List<IPropertySymbol> sourceProperties,
        EquatableArray<string> includedMemberNames,
        MemberConfigReference? fluentConfig)
    {
        foreach (var memberName in includedMemberNames)
        {
            var navProp = sourceProperties.FirstOrDefault(
                sp => string.Equals(sp.Name, memberName, StringComparison.Ordinal));

            if (navProp is null)
                continue;

            if (navProp.Type is not INamedTypeSymbol navType)
                continue;

            var subProperties = TypeSymbolHelper.GetAllPublicProperties(navType);

            // Try exact name match on the sub-properties
            var subMatch = subProperties.FirstOrDefault(
                sp => string.Equals(sp.Name, destProp.Name, StringComparison.OrdinalIgnoreCase));

            if (subMatch is null)
                continue;

            var convKind = ConversionResolver.DetermineConversion(compilation, subMatch.Type, destProp.Type);
            // Use ?. for nullable navigation properties, . for non-nullable
            var navAccessor = navProp.Type.NullableAnnotation == NullableAnnotation.Annotated
                || navProp.Type.IsValueType
                    ? "?." : ".";
            var accessPath = navProp.Name + navAccessor + subMatch.Name;
            var isInitOnly = destProp.SetMethod is not null && destProp.SetMethod.IsInitOnly;

            var match = new PropertyMatchDescriptor(
                accessPath,
                TypeSymbolHelper.GetFullTypeName(subMatch.Type),
                destProp.Name,
                TypeSymbolHelper.GetFullTypeName(destProp.Type),
                convKind,
                null, null, CollectionKind.None, null, null, null,
                null, null, null, null, null, null,
                isInitOnly);

            if (fluentConfig is not null)
                match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);

            return match;
        }

        return null;
    }
}
