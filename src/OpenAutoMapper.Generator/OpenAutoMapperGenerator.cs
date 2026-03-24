using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using OpenAutoMapper.Generator.Diagnostics;
using OpenAutoMapper.Generator.Models;
using OpenAutoMapper.Generator.Pipeline;

namespace OpenAutoMapper.Generator;

[Generator]
public sealed class OpenAutoMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Stage 1a: Find all Profile subclasses
        var profileDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => ProfileDiscovery.IsProfileCandidate(node),
                transform: static (ctx, ct) => ProfileDiscovery.GetProfileInfo(ctx, ct))
            .Where(static p => p is not null)
            .Select(static (p, _) => p!);

        // Stage 1b: Find all [AutoMap] attributed classes
        var autoMapDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => ProfileDiscovery.IsAutoMapCandidate(node),
                transform: static (ctx, ct) => ProfileDiscovery.GetAutoMapInfo(ctx, ct))
            .Where(static p => p is not null)
            .Select(static (p, _) => p!);

        // Merge both streams into a single collection
        var allProfiles = profileDeclarations.Collect()
            .Combine(autoMapDeclarations.Collect());

        // Stage 2: Combine with compilation and analyze
        var compilationAndProfiles = context.CompilationProvider.Combine(allProfiles);

        // Stage 3: Generate mapping code
        context.RegisterSourceOutput(compilationAndProfiles,
            static (spc, source) =>
            {
                var compilation = source.Left;
                var profileInfos = source.Right.Left;
                var autoMapInfos = source.Right.Right;

                // Merge both arrays
                var merged = profileInfos.AddRange(autoMapInfos);
                Execute(compilation, merged, spc);
            });
    }

    private static void Execute(Compilation compilation, ImmutableArray<ProfileInfo> profiles, SourceProductionContext context)
    {
        if (profiles.IsDefaultOrEmpty)
            return;

        // Analyze all profiles once
        var allTypePairs = new System.Collections.Generic.List<TypePairDescriptor>();
        foreach (var profile in profiles)
        {
            var typePairs = PropertyAnalyzer.AnalyzeProfile(compilation, profile, context);
            allTypePairs.AddRange(typePairs);
        }

        // Detect circular references across all type pairs
        PropertyAnalyzer.DetectCycles(allTypePairs, context);

        // Split into mapping and projection pairs
        var mappingPairs = allTypePairs.FindAll(tp => !tp.IsProjection);
        var projectionPairs = allTypePairs.FindAll(tp => tp.IsProjection);

        // Validate projection constraints
        foreach (var proj in projectionPairs)
        {
            if (proj.HasCyclicReference)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CircularReferenceInProjection,
                    Location.None,
                    proj.SourceFullName,
                    proj.DestFullName));
            }

            if (proj.IsPolymorphicBase)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.PolymorphicIncludeInProjection,
                    Location.None,
                    proj.SourceFullName,
                    proj.DestFullName));
            }
        }

        // Emit mapping extension methods for each mapping type pair
        foreach (var typePair in mappingPairs)
        {
            var code = MappingCodeEmitter.EmitMappingExtension(typePair);
            if (code is not null)
            {
                var nameSuffix = typePair.MappingName is not null ? "_" + typePair.MappingName : "";
                context.AddSource(
                    $"{typePair.SourceName}To{typePair.DestName}{nameSuffix}MappingExtensions.g.cs",
                    SourceText.From(code, Encoding.UTF8));
            }
        }

        // Emit projection expressions for each projection type pair
        foreach (var typePair in projectionPairs)
        {
            if (typePair.HasCyclicReference || typePair.IsPolymorphicBase)
                continue; // Skip errored projections

            var code = ProjectionExpressionEmitter.EmitProjectionExpression(typePair, projectionPairs, context);
            if (code is not null)
            {
                context.AddSource(
                    $"{typePair.SourceName}To{typePair.DestName}ProjectionExpressions.g.cs",
                    SourceText.From(code, Encoding.UTF8));
            }
        }

        // Emit ProjectTo<T> generic dispatch (if any projections)
        var validProjections = projectionPairs.FindAll(tp =>
            !tp.HasCyclicReference && !tp.IsPolymorphicBase && tp.PropertyMatches.Length > 0);
        if (validProjections.Count > 0)
        {
            var projectToCode = ProjectToRegistryEmitter.EmitProjectToDispatch(validProjections);
            context.AddSource("OpenAutoMapperQueryableExtensions.g.cs",
                SourceText.From(projectToCode, Encoding.UTF8));
        }

        // Emit IMapper implementation (only include mapping pairs with matches)
        var mappableTypePairs = mappingPairs.FindAll(tp => tp.PropertyMatches.Length > 0);
        if (mappableTypePairs.Count > 0)
        {
            var mapperImpl = MapperImplEmitter.EmitMapperImplementation(mappableTypePairs);
            context.AddSource("OpenAutoMapperImpl.g.cs", SourceText.From(mapperImpl, Encoding.UTF8));

            var factoryInit = MapperImplEmitter.EmitFactoryInitializer();
            context.AddSource("OpenAutoMapperFactoryInit.g.cs", SourceText.From(factoryInit, Encoding.UTF8));
        }
    }
}
