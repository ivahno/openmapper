using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Matches properties by exact name and case-insensitive name.
/// </summary>
internal static class ExactNameMatcher
{
    public static PropertyMatchDescriptor? TryMatch(
        Compilation compilation,
        IPropertySymbol destProp,
        List<IPropertySymbol> sourceProperties,
        MemberConfigReference? fluentConfig)
    {
        // Try exact name match
        var exactMatch = sourceProperties.FirstOrDefault(
            sp => string.Equals(sp.Name, destProp.Name, StringComparison.Ordinal));

        if (exactMatch is not null)
        {
            var convKind = ConversionResolver.DetermineConversion(compilation, exactMatch.Type, destProp.Type);
            var match = PropertyMatchFactory.CreatePropertyMatch(compilation, exactMatch.Name, exactMatch.Type, destProp, convKind);
            if (fluentConfig is not null)
                match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);
            return match;
        }

        // Try case-insensitive match
        var caseInsensitiveMatch = sourceProperties.FirstOrDefault(
            sp => string.Equals(sp.Name, destProp.Name, StringComparison.OrdinalIgnoreCase));

        if (caseInsensitiveMatch is not null)
        {
            var convKind = ConversionResolver.DetermineConversion(compilation, caseInsensitiveMatch.Type, destProp.Type);
            var match = PropertyMatchFactory.CreatePropertyMatch(compilation, caseInsensitiveMatch.Name, caseInsensitiveMatch.Type, destProp, convKind);
            if (fluentConfig is not null)
                match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);
            return match;
        }

        return null;
    }
}
