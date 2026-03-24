using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Diagnostics;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Selects the best constructor for a destination type and matches parameters to source properties.
/// </summary>
internal static class ConstructorMatcher
{
    /// <summary>
    /// Attempts to find a constructor match for the destination type.
    /// Returns matched constructor parameter descriptors and the set of source property names consumed.
    /// </summary>
    public static EquatableArray<ConstructorParamDescriptor> MatchConstructorParams(
        Compilation compilation,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        EquatableArray<CtorParamConfigReference> ctorParamConfigs,
        List<IPropertySymbol> sourceProperties,
        SourceProductionContext context,
        out HashSet<string> consumedDestPropertyNames)
    {
        consumedDestPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Skip if dest has an accessible parameterless constructor and no explicit ForCtorParam configs
        var constructors = destType.Constructors
            .Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        if (constructors.Count == 0)
            return new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);

        var hasParameterless = constructors.Any(c => c.Parameters.Length == 0);

        // If there's explicit ForCtorParam config, find matching constructor
        if (ctorParamConfigs.Length > 0)
        {
            var configParamNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cfg in ctorParamConfigs)
                configParamNames.Add(cfg.ParamName);

            // Find ctor whose params are a superset of the configured params
            var bestCtor = constructors
                .Where(c => c.Parameters.Length > 0)
                .Where(c => configParamNames.All(cpn =>
                    c.Parameters.Any(p => string.Equals(p.Name, cpn, StringComparison.OrdinalIgnoreCase))))
                .OrderByDescending(c => c.Parameters.Length)
                .FirstOrDefault();

            if (bestCtor is not null)
            {
                return MatchCtorParams(compilation, bestCtor, sourceProperties, ctorParamConfigs, consumedDestPropertyNames);
            }

            // Report diagnostic: ForCtorParam param doesn't match any constructor
            foreach (var cfg in ctorParamConfigs)
            {
                var found = constructors.Any(c =>
                    c.Parameters.Any(p => string.Equals(p.Name, cfg.ParamName, StringComparison.OrdinalIgnoreCase)));
                if (!found)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.CtorParamNotFound,
                        Location.None,
                        cfg.ParamName,
                        destType.Name));
                }
            }
        }

        // No explicit config: auto-detect best constructor
        // Skip if parameterless ctor exists (prefer it for backwards compatibility)
        if (hasParameterless)
            return new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);

        // Pick the constructor with the most parameters that ALL match source properties by name
        var sourcePropertyNames = new HashSet<string>(
            sourceProperties.Select(p => p.Name),
            StringComparer.OrdinalIgnoreCase);

        var autoMatchCtor = constructors
            .Where(c => c.Parameters.Length > 0)
            .Where(c => c.Parameters.All(p => sourcePropertyNames.Contains(p.Name)))
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (autoMatchCtor is not null)
        {
            return MatchCtorParams(compilation, autoMatchCtor, sourceProperties, ctorParamConfigs, consumedDestPropertyNames);
        }

        // No parameterless ctor and no auto-match → diagnostic
        if (!hasParameterless)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NoMatchingConstructor,
                Location.None,
                destType.Name,
                sourceType.Name));
        }

        return new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);
    }

    private static EquatableArray<ConstructorParamDescriptor> MatchCtorParams(
        Compilation compilation,
        IMethodSymbol constructor,
        List<IPropertySymbol> sourceProperties,
        EquatableArray<CtorParamConfigReference> ctorParamConfigs,
        HashSet<string> consumedDestPropertyNames)
    {
        var result = new List<ConstructorParamDescriptor>();

        foreach (var param in constructor.Parameters)
        {
            // Check if there's an explicit ForCtorParam mapping
            string? sourceMemberName = null;
            foreach (var cfg in ctorParamConfigs)
            {
                if (string.Equals(cfg.ParamName, param.Name, StringComparison.OrdinalIgnoreCase))
                {
                    sourceMemberName = cfg.SourceMemberName;
                    break;
                }
            }

            // Find source property
            IPropertySymbol? sourceProp;
            if (sourceMemberName is not null)
            {
                sourceProp = sourceProperties.FirstOrDefault(
                    sp => string.Equals(sp.Name, sourceMemberName, StringComparison.Ordinal));
            }
            else
            {
                // Auto-match by name (case-insensitive)
                sourceProp = sourceProperties.FirstOrDefault(
                    sp => string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (sourceProp is null)
                continue;

            var convKind = ConversionResolver.DetermineConversion(compilation, sourceProp.Type, param.Type);

            result.Add(new ConstructorParamDescriptor(
                param.Name,
                TypeSymbolHelper.GetFullTypeName(param.Type),
                sourceProp.Name,
                TypeSymbolHelper.GetFullTypeName(sourceProp.Type),
                convKind));

            // Mark this dest property name as consumed (so it's not also assigned via property setter)
            consumedDestPropertyNames.Add(param.Name);
        }

        return new EquatableArray<ConstructorParamDescriptor>(result.ToImmutableArray());
    }
}
