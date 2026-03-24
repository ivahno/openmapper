using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Diagnostics;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;
using OpenAutoMapper.Generator.Pipeline.Matching;

namespace OpenAutoMapper.Generator.Pipeline;

/// <summary>
/// Stage 2: Resolves source/destination types and matches their properties by name.
/// Orchestrates matching strategies defined in Pipeline/Matching/.
/// </summary>
internal static class PropertyAnalyzer
{
    private const string IgnoreMapAttributeName = "OpenAutoMapper.IgnoreMapAttribute";
    private const string IgnoreAttributeName = "OpenAutoMapper.IgnoreAttribute";
    private const string SensitivePropertyAttributeName = "OpenAutoMapper.SensitivePropertyAttribute";
    private const string MapFromAttributeName = "OpenAutoMapper.MapFromAttribute";

    public static List<TypePairDescriptor> AnalyzeProfile(
        Compilation compilation,
        ProfileInfo profile,
        SourceProductionContext context)
    {
        var results = new List<TypePairDescriptor>();

        foreach (var pair in profile.TypePairs)
        {
            var sourceType = compilation.GetTypeByMetadataName(pair.SourceFullName);
            var destType = compilation.GetTypeByMetadataName(pair.DestFullName);

            if (sourceType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.SourceTypeUnknown,
                    Location.None,
                    pair.SourceFullName,
                    pair.DestFullName));
                continue;
            }

            if (destType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.DestTypeUnknown,
                    Location.None,
                    pair.SourceFullName,
                    pair.DestFullName));
                continue;
            }

            // Check for open generics
            if (sourceType.IsUnboundGenericType || destType.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.OpenGenericNotSupported,
                    Location.None,
                    sourceType.Name,
                    destType.Name));
                continue;
            }

            // Check for interface targets
            if (destType.TypeKind == TypeKind.Interface)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InterfaceTargetNotSupported,
                    Location.None,
                    destType.Name));
                continue;
            }

            // Validate MaxDepth
            if (pair.MaxDepth.HasValue && pair.MaxDepth.Value <= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidMaxDepth,
                    Location.None,
                    pair.MaxDepth.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    pair.SourceFullName,
                    pair.DestFullName));
                continue;
            }

            // Check if this is a dictionary-to-object mapping
            if (DictionaryToObjectMatcher.IsStringKeyedDictionary(sourceType, out var dictValueType))
            {
                var dictMatches = DictionaryToObjectMatcher.MatchDictionaryToObject(
                    compilation, sourceType, destType, dictValueType!);

                var sourceFullNameDict = TypeSymbolHelper.GetFullTypeName(sourceType);
                var destFullNameDict = TypeSymbolHelper.GetFullTypeName(destType);

                results.Add(new TypePairDescriptor(
                    sourceFullNameDict,
                    sourceType.Name,
                    destFullNameDict,
                    destType.Name,
                    new EquatableArray<PropertyMatchDescriptor>(dictMatches.ToImmutableArray()),
                    new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty),
                    false,
                    pair.MaxDepth ?? 10,
                    pair.IsProjection,
                    pair.BeforeMapExpression,
                    pair.AfterMapExpression,
                    pair.ConstructUsingExpression,
                    pair.ConvertUsingExpression,
                    new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty),
                    pair.EnumMappingStrategy,
                    profile.AllowNullCollections,
                    pair.IsDeepClone,
                    pair.AdditionalSourceTypes,
                    pair.MappingName));
                continue;
            }

            // Match constructor params (before property matching so we know which props are ctor-consumed)
            var sourceProperties = TypeSymbolHelper.GetAllPublicProperties(sourceType);
            HashSet<string> ctorConsumedNames;
            EquatableArray<ConstructorParamDescriptor> ctorParams;
            if (pair.ConstructUsingExpression is null)
            {
                ctorParams = ConstructorMatcher.MatchConstructorParams(
                    compilation, sourceType, destType, pair.CtorParamConfigs,
                    sourceProperties, context, out ctorConsumedNames);
            }
            else
            {
                ctorParams = new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);
                ctorConsumedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var matches = MatchProperties(compilation, sourceType, destType, pair.MemberConfigs, context,
                profile.Prefixes, profile.Postfixes, ctorConsumedNames, pair.ForAllMembersConfig,
                pair.IncludedMemberNames);

            var sourceFullName = TypeSymbolHelper.GetFullTypeName(sourceType);
            var destFullName = TypeSymbolHelper.GetFullTypeName(destType);

            // Resolve Include types
            var includedDerived = ResolveIncludedTypes(compilation, pair.IncludedDerivedTypes,
                sourceType, destType, context, isBase: false);

            int maxDepth = pair.MaxDepth ?? 10;

            results.Add(new TypePairDescriptor(
                sourceFullName,
                sourceType.Name,
                destFullName,
                destType.Name,
                new EquatableArray<PropertyMatchDescriptor>(matches.ToImmutableArray()),
                new EquatableArray<IncludedTypeDescriptor>(includedDerived.ToImmutableArray()),
                false,
                maxDepth,
                pair.IsProjection,
                pair.BeforeMapExpression,
                pair.AfterMapExpression,
                pair.ConstructUsingExpression,
                pair.ConvertUsingExpression,
                ctorParams,
                pair.EnumMappingStrategy,
                profile.AllowNullCollections,
                pair.IsDeepClone,
                pair.AdditionalSourceTypes,
                pair.MappingName));
        }

        // Resolve IncludeBase: for each pair with IncludedBaseTypes, find the base descriptor
        // and add current pair to its IncludedDerivedTypes
        ResolveIncludeBase(compilation, profile, results, context);

        return results;
    }

    /// <summary>
    /// Detects circular references in the type pair graph.
    /// Delegates to <see cref="CycleDetector"/>.
    /// </summary>
    public static void DetectCycles(List<TypePairDescriptor> allTypePairs, SourceProductionContext context)
    {
        CycleDetector.DetectCycles(allTypePairs, context);
    }

    private static List<IncludedTypeDescriptor> ResolveIncludedTypes(
        Compilation compilation,
        EquatableArray<IncludedTypeReference> refs,
        INamedTypeSymbol baseSourceType,
        INamedTypeSymbol baseDestType,
        SourceProductionContext context,
        bool isBase)
    {
        var result = new List<IncludedTypeDescriptor>();

        foreach (var r in refs)
        {
            var srcType = compilation.GetTypeByMetadataName(r.SourceFullName);
            var dstType = compilation.GetTypeByMetadataName(r.DestFullName);

            if (srcType is null || dstType is null)
                continue;

            if (!isBase)
            {
                // For Include: verify derived source inherits from base source
                if (!InheritsFrom(srcType, baseSourceType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.IncludeTypeMismatch,
                        Location.None,
                        r.SourceFullName,
                        r.DestFullName,
                        TypeSymbolHelper.GetFullTypeName(baseSourceType)));
                    continue;
                }
            }

            result.Add(new IncludedTypeDescriptor(
                TypeSymbolHelper.GetFullTypeName(srcType),
                srcType.Name,
                TypeSymbolHelper.GetFullTypeName(dstType),
                dstType.Name));
        }

        return result;
    }

    private static void ResolveIncludeBase(
        Compilation compilation,
        ProfileInfo profile,
        List<TypePairDescriptor> results,
        SourceProductionContext context)
    {
        // Build a lookup of results by source+dest full name
        var resultLookup = new Dictionary<string, TypePairDescriptor>(StringComparer.Ordinal);
        foreach (var r in results)
        {
            var key = r.SourceFullName + "->" + r.DestFullName;
            resultLookup[key] = r;
        }

        // For each TypePairReference with IncludeBase entries, find the base descriptor and add current as derived
        foreach (var pair in profile.TypePairs)
        {
            if (pair.IncludedBaseTypes.Length == 0)
                continue;

            var currentKey = pair.SourceFullName + "->" + pair.DestFullName;
            if (!resultLookup.TryGetValue(currentKey, out var currentDescriptor))
                continue;

            foreach (var baseRef in pair.IncludedBaseTypes)
            {
                var baseKey = baseRef.SourceFullName + "->" + baseRef.DestFullName;
                if (resultLookup.TryGetValue(baseKey, out var baseDescriptor))
                {
                    // Add current as a derived type of the base
                    var newDerived = new List<IncludedTypeDescriptor>(baseDescriptor.IncludedDerivedTypes);
                    newDerived.Add(new IncludedTypeDescriptor(
                        currentDescriptor.SourceFullName,
                        currentDescriptor.SourceName,
                        currentDescriptor.DestFullName,
                        currentDescriptor.DestName));

                    // We need to rebuild the descriptor (IncludedDerivedTypes is immutable via constructor)
                    var idx = results.IndexOf(baseDescriptor);
                    if (idx >= 0)
                    {
                        results[idx] = new TypePairDescriptor(
                            baseDescriptor.SourceFullName,
                            baseDescriptor.SourceName,
                            baseDescriptor.DestFullName,
                            baseDescriptor.DestName,
                            baseDescriptor.PropertyMatches,
                            new EquatableArray<IncludedTypeDescriptor>(newDerived.ToImmutableArray()),
                            baseDescriptor.HasCyclicReference,
                            baseDescriptor.MaxDepth,
                            baseDescriptor.IsProjection,
                            baseDescriptor.BeforeMapExpression,
                            baseDescriptor.AfterMapExpression,
                            baseDescriptor.ConstructUsingExpression,
                            baseDescriptor.ConvertUsingExpression);
                        resultLookup[baseKey] = results[idx];
                    }
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.IncludeBaseTypeMismatch,
                        Location.None,
                        baseRef.SourceFullName,
                        baseRef.DestFullName,
                        pair.SourceFullName,
                        pair.DestFullName));
                }
            }
        }
    }

    private static bool InheritsFrom(INamedTypeSymbol derived, INamedTypeSymbol baseType)
    {
        var current = derived.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static List<PropertyMatchDescriptor> MatchProperties(
        Compilation compilation,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        EquatableArray<MemberConfigReference> memberConfigs,
        SourceProductionContext context,
        EquatableArray<string> prefixes,
        EquatableArray<string> postfixes,
        HashSet<string> ctorConsumedNames,
        MemberConfigReference? forAllMembersConfig,
        EquatableArray<string> includedMemberNames)
    {
        var matches = new List<PropertyMatchDescriptor>();

        var sourceProperties = TypeSymbolHelper.GetAllPublicProperties(sourceType);
        var destProperties = TypeSymbolHelper.GetAllPublicProperties(destType);

        // Track which dest property names have been matched (for path-based and unflatten)
        var matchedDestNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var destProp in destProperties)
        {
            // Skip properties that cannot be set (allow init-only setters)
            if (destProp.SetMethod is null || destProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Skip properties already consumed by constructor mapping
            if (ctorConsumedNames.Contains(destProp.Name))
            {
                matchedDestNames.Add(destProp.Name);
                continue;
            }

            // Check for [IgnoreMap] or [Ignore] on destination
            if (TypeSymbolHelper.HasAttribute(destProp, IgnoreMapAttributeName) || TypeSymbolHelper.HasAttribute(destProp, IgnoreAttributeName))
                continue;

            // Check fluent MemberConfigs for this destination property
            var fluentConfig = FindMemberConfig(memberConfigs, destProp.Name);
            if (fluentConfig is not null)
            {
                if (fluentConfig.IsIgnored)
                {
                    matchedDestNames.Add(destProp.Name);
                    continue;
                }

                if (fluentConfig.MemberValueResolverTypeName is not null)
                {
                    // IMemberValueResolver: opt.MapFrom<TResolver, TSourceMember>(s => s.Prop)
                    var match = new PropertyMatchDescriptor(
                        fluentConfig.SourceMemberName ?? destProp.Name,
                        TypeSymbolHelper.GetFullTypeName(destProp.Type),
                        destProp.Name,
                        TypeSymbolHelper.GetFullTypeName(destProp.Type),
                        ConversionKind.Direct,
                        null, null, CollectionKind.None, null, null, null,
                        fluentConfig.ConditionExpression,
                        fluentConfig.PreConditionExpression,
                        fluentConfig.NullSubstituteExpression,
                        null,
                        null,
                        fluentConfig.MemberValueResolverTypeName);
                    matches.Add(match);
                    matchedDestNames.Add(destProp.Name);
                    continue;
                }

                if (fluentConfig.ValueResolverTypeName is not null)
                {
                    // Value resolver: opt.MapFrom<TResolver>()
                    var match = new PropertyMatchDescriptor(
                        destProp.Name,
                        TypeSymbolHelper.GetFullTypeName(destProp.Type),
                        destProp.Name,
                        TypeSymbolHelper.GetFullTypeName(destProp.Type),
                        ConversionKind.Direct,
                        null, null, CollectionKind.None, null, null, null,
                        fluentConfig.ConditionExpression,
                        fluentConfig.PreConditionExpression,
                        fluentConfig.NullSubstituteExpression,
                        fluentConfig.ValueResolverTypeName);
                    matches.Add(match);
                    matchedDestNames.Add(destProp.Name);
                    continue;
                }

                if (fluentConfig.SourceMemberName is not null)
                {
                    var mappedSource = sourceProperties.FirstOrDefault(
                        sp => string.Equals(sp.Name, fluentConfig.SourceMemberName, StringComparison.Ordinal));

                    if (mappedSource is not null)
                    {
                        var convKind = ConversionResolver.DetermineConversion(compilation, mappedSource.Type, destProp.Type);
                        var match = PropertyMatchFactory.CreatePropertyMatch(compilation, mappedSource.Name, mappedSource.Type, destProp, convKind);
                        match = PropertyMatchFactory.WithMemberConfig(match, fluentConfig);
                        matches.Add(match);
                        matchedDestNames.Add(destProp.Name);
                        continue;
                    }

                    // Source member configured but not resolvable as a direct property
                    // (e.g., s.Lines.Count — a navigation chain). Suppress unmapped warning
                    // since the user explicitly configured this member via ForMember/MapFrom.
                    matchedDestNames.Add(destProp.Name);
                    continue;
                }

                // Config has condition/nullsubstitute but no explicit source mapping — still apply to auto-matched property below
            }

            // Check for [SensitiveProperty] on destination -- must be explicitly configured
            if (TypeSymbolHelper.HasAttribute(destProp, SensitivePropertyAttributeName))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.SensitivePropertyMapped,
                    Location.None,
                    destProp.Name,
                    destType.Name));
                continue;
            }

            // Check for [MapFrom] on destination
            var mapFromAttr = TypeSymbolHelper.GetAttribute(destProp, MapFromAttributeName);
            if (mapFromAttr is not null)
            {
                var sourceNameArg = mapFromAttr.ConstructorArguments.FirstOrDefault();
                if (sourceNameArg.Value is string sourceMemberName)
                {
                    var mappedSource = sourceProperties.FirstOrDefault(
                        sp => string.Equals(sp.Name, sourceMemberName, StringComparison.Ordinal));

                    if (mappedSource is not null)
                    {
                        var convKind = ConversionResolver.DetermineConversion(compilation, mappedSource.Type, destProp.Type);
                        matches.Add(PropertyMatchFactory.CreatePropertyMatch(compilation, mappedSource.Name, mappedSource.Type, destProp, convKind));
                        matchedDestNames.Add(destProp.Name);
                        continue;
                    }
                }
            }

            // Try exact name + case-insensitive match
            var exactOrCiMatch = ExactNameMatcher.TryMatch(compilation, destProp, sourceProperties, fluentConfig);
            if (exactOrCiMatch is not null)
            {
                matches.Add(exactOrCiMatch);
                matchedDestNames.Add(destProp.Name);
                continue;
            }

            // Try prefix/postfix stripping
            var prefixPostfixMatch = PrefixPostfixMatcher.TryMatch(compilation, destProp, sourceProperties, fluentConfig, prefixes, postfixes);
            if (prefixPostfixMatch is not null)
            {
                matches.Add(prefixPostfixMatch);
                matchedDestNames.Add(destProp.Name);
                continue;
            }

            // Try flattening: e.g., dest "AddressCity" -> source "Address.City"
            var flattenMatch = FlattenMatcher.TryFlattenMatch(sourceProperties, destProp.Name, sourceType);
            if (flattenMatch is not null)
            {
                var convKind = ConversionResolver.DetermineConversion(compilation, flattenMatch.Value.leafType, destProp.Type);
                matches.Add(new PropertyMatchDescriptor(
                    flattenMatch.Value.accessPath,
                    TypeSymbolHelper.GetFullTypeName(flattenMatch.Value.leafType),
                    destProp.Name,
                    TypeSymbolHelper.GetFullTypeName(destProp.Type),
                    convKind));
                matchedDestNames.Add(destProp.Name);
                continue;
            }

            // Try IncludeMembers matching: for unmatched dest props, try each included member's sub-properties
            if (includedMemberNames.Length > 0)
            {
                var includeMembersMatch = IncludeMembersMatcher.TryMatch(
                    compilation, destProp, sourceProperties, includedMemberNames, fluentConfig);
                if (includeMembersMatch is not null)
                {
                    matches.Add(includeMembersMatch);
                    matchedDestNames.Add(destProp.Name);
                    continue;
                }
            }

            // Unmapped property -- report warning (may be resolved by ForPath or unflattening below)
            // Defer: we'll check if path-based configs or unflattening covers it
        }

        // Process path-based MemberConfigReference entries (ForPath with dotted DestMemberName)
        foreach (var config in memberConfigs)
        {
            if (!config.DestMemberName.Contains("."))
                continue;

            if (config.IsIgnored)
            {
                // ForPath ignore — suppress unmapped warning for the first segment
                var firstSegment = config.DestMemberName.Substring(0, config.DestMemberName.IndexOf('.'));
                matchedDestNames.Add(firstSegment);
                continue;
            }

            // Resolve the path through the dest type's property chain
            var pathSegments = config.DestMemberName.Split('.');
            var intermediateTypes = new List<string>();
            var currentType = destType;
            bool pathValid = true;

            for (int i = 0; i < pathSegments.Length - 1; i++)
            {
                var segmentProp = TypeSymbolHelper.GetAllPublicProperties(currentType).FirstOrDefault(
                    p => string.Equals(p.Name, pathSegments[i], StringComparison.Ordinal));

                if (segmentProp is null || segmentProp.Type is not INamedTypeSymbol segmentType)
                {
                    pathValid = false;
                    break;
                }

                intermediateTypes.Add(TypeSymbolHelper.GetFullTypeName(segmentType));
                currentType = segmentType;
            }

            if (!pathValid)
                continue;

            // Resolve the leaf property
            var leafSegment = pathSegments[pathSegments.Length - 1];
            var leafProp = TypeSymbolHelper.GetAllPublicProperties(currentType).FirstOrDefault(
                p => string.Equals(p.Name, leafSegment, StringComparison.Ordinal));

            if (leafProp is null)
                continue;

            // Resolve source property
            string sourcePropertyName = config.SourceMemberName ?? leafSegment;
            string sourcePropertyType = TypeSymbolHelper.GetFullTypeName(leafProp.Type);
            var conversionKind = ConversionKind.Direct;

            if (config.SourceMemberName is not null)
            {
                var sourceProp = sourceProperties.FirstOrDefault(
                    sp => string.Equals(sp.Name, config.SourceMemberName, StringComparison.Ordinal));
                if (sourceProp is not null)
                {
                    sourcePropertyType = TypeSymbolHelper.GetFullTypeName(sourceProp.Type);
                    conversionKind = ConversionResolver.DetermineConversion(compilation, sourceProp.Type, leafProp.Type);
                }
            }

            var pathMatch = new PropertyMatchDescriptor(
                sourcePropertyName,
                sourcePropertyType,
                config.DestMemberName,
                TypeSymbolHelper.GetFullTypeName(leafProp.Type),
                conversionKind,
                null, null, CollectionKind.None, null, null, null,
                config.ConditionExpression,
                config.PreConditionExpression,
                config.NullSubstituteExpression,
                config.ValueResolverTypeName,
                string.Join("|", intermediateTypes),
                null);

            matches.Add(pathMatch);
            // Mark the first segment as matched to suppress unmapped warnings
            matchedDestNames.Add(pathSegments[0]);
        }

        // Try unflattening for unmapped complex dest properties
        foreach (var destProp in destProperties)
        {
            if (destProp.SetMethod is null || destProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (matchedDestNames.Contains(destProp.Name))
                continue;
            if (TypeSymbolHelper.HasAttribute(destProp, IgnoreMapAttributeName) || TypeSymbolHelper.HasAttribute(destProp, IgnoreAttributeName))
                continue;

            var unflattenMatches = UnflattenMatcher.TryUnflattenMatch(compilation, sourceProperties, destProp, destType);
            if (unflattenMatches is not null && unflattenMatches.Count > 0)
            {
                matches.AddRange(unflattenMatches);
                matchedDestNames.Add(destProp.Name);
            }
        }

        // Report unmapped warnings for properties not matched by any strategy
        foreach (var destProp in destProperties)
        {
            if (destProp.SetMethod is null || destProp.SetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (matchedDestNames.Contains(destProp.Name))
                continue;
            if (TypeSymbolHelper.HasAttribute(destProp, IgnoreMapAttributeName) || TypeSymbolHelper.HasAttribute(destProp, IgnoreAttributeName))
                continue;
            if (TypeSymbolHelper.HasAttribute(destProp, SensitivePropertyAttributeName))
                continue;

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnmappedDestProperty,
                Location.None,
                destProp.Name,
                destType.Name,
                sourceType.Name));
        }

        // Apply ForAllMembers config to all matched properties without explicit ForMember override
        if (forAllMembersConfig is not null)
        {
            var explicitDestNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var config in memberConfigs)
            {
                explicitDestNames.Add(config.DestMemberName);
            }

            for (int i = 0; i < matches.Count; i++)
            {
                if (!explicitDestNames.Contains(matches[i].DestPropertyName))
                {
                    matches[i] = PropertyMatchFactory.WithMemberConfig(matches[i], forAllMembersConfig);
                }
            }
        }

        return matches;
    }

    private static MemberConfigReference? FindMemberConfig(
        EquatableArray<MemberConfigReference> memberConfigs,
        string destPropertyName)
    {
        foreach (var config in memberConfigs)
        {
            if (string.Equals(config.DestMemberName, destPropertyName, StringComparison.Ordinal))
                return config;
        }
        return null;
    }
}
