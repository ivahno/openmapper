using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Matches properties by stripping recognized prefixes or postfixes.
/// </summary>
internal static class PrefixPostfixMatcher
{
    public static PropertyMatchDescriptor? TryMatch(
        Compilation compilation,
        IPropertySymbol destProp,
        List<IPropertySymbol> sourceProperties,
        MemberConfigReference? fluentConfig,
        EquatableArray<string> prefixes,
        EquatableArray<string> postfixes)
    {
        // Try prefix stripping: source "GetName" matches dest "Name" with prefix "Get"
        IPropertySymbol? prefixMatch = null;
        if (prefixes.Length > 0)
        {
            foreach (var prefix in prefixes)
            {
                prefixMatch = sourceProperties.FirstOrDefault(
                    sp => string.Equals(sp.Name, prefix + destProp.Name, StringComparison.OrdinalIgnoreCase));
                if (prefixMatch is not null)
                    break;
            }
        }

        if (prefixMatch is not null)
        {
            var convKind = ConversionResolver.DetermineConversion(compilation, prefixMatch.Type, destProp.Type);
            var match = PropertyMatchFactory.CreatePropertyMatch(prefixMatch.Name, prefixMatch.Type, destProp, convKind);
            if (fluentConfig is not null)
                match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);
            return match;
        }

        // Try postfix stripping: source "NameDto" matches dest "Name" with postfix "Dto"
        IPropertySymbol? postfixMatch = null;
        if (postfixes.Length > 0)
        {
            foreach (var postfix in postfixes)
            {
                postfixMatch = sourceProperties.FirstOrDefault(
                    sp => string.Equals(sp.Name, destProp.Name + postfix, StringComparison.OrdinalIgnoreCase));
                if (postfixMatch is not null)
                    break;
            }
        }

        if (postfixMatch is not null)
        {
            var convKind = ConversionResolver.DetermineConversion(compilation, postfixMatch.Type, destProp.Type);
            var match = PropertyMatchFactory.CreatePropertyMatch(postfixMatch.Name, postfixMatch.Type, destProp, convKind);
            if (fluentConfig is not null)
                match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);
            return match;
        }

        return null;
    }
}
