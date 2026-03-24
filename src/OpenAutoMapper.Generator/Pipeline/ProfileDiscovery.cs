using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline;

/// <summary>
/// Stage 1: Discovers Profile subclasses via syntax and semantic analysis.
/// </summary>
internal static class ProfileDiscovery
{
    private const string ProfileFullName = "OpenAutoMapper.Profile";
    private const string AutoMapAttributeFullName = "OpenAutoMapper.AutoMapAttribute";

    /// <summary>
    /// Fast syntactic check: is this node a class declaration with a base type?
    /// This runs on every syntax node so it must be very cheap.
    /// </summary>
    public static bool IsProfileCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
            && classDecl.BaseList is not null
            && classDecl.BaseList.Types.Count > 0;
    }

    /// <summary>
    /// Semantic check: does this class actually inherit from OpenAutoMapper.Profile?
    /// If so, extract CreateMap calls from the constructor body.
    /// </summary>
    public static ProfileInfo? GetProfileInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        ct.ThrowIfCancellationRequested();

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct);
        if (classSymbol is null)
            return null;

        // Check if this class inherits from OpenAutoMapper.Profile
        if (!InheritsFromProfile(classSymbol))
            return null;

        // Find CreateMap calls in the constructor
        var typePairs = ExtractCreateMapCalls(classDecl, semanticModel, ct);

        if (typePairs.Length == 0)
            return null;

        // Extract RecognizePrefixes/RecognizePostfixes calls
        var (prefixes, postfixes) = ExtractPrefixesAndPostfixes(classDecl);

        // Detect AllowNullCollections = true assignment
        bool allowNullCollections = DetectAllowNullCollections(classDecl);

        var className = classSymbol.Name;
        var namespaceName = GetFullNamespace(classSymbol);
        var location = classDecl.GetLocation();
        var filePath = location.SourceTree?.FilePath ?? string.Empty;
        var line = location.GetLineSpan().StartLinePosition.Line;

        return new ProfileInfo(className, namespaceName, typePairs, filePath, line, prefixes, postfixes, allowNullCollections);
    }

    /// <summary>
    /// Fast syntactic check: is this node a class with an attribute list?
    /// </summary>
    public static bool IsAutoMapCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
            && classDecl.AttributeLists.Count > 0;
    }

    /// <summary>
    /// Semantic check: does this class have [AutoMap(typeof(Source))]?
    /// If so, create a synthetic ProfileInfo from the attribute.
    /// </summary>
    public static ProfileInfo? GetAutoMapInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        ct.ThrowIfCancellationRequested();

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct);
        if (classSymbol is null)
            return null;

        // Find [AutoMap] attribute
        var autoMapAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a =>
            {
                if (a.AttributeClass is null) return false;
                var name = GetFullTypeName(a.AttributeClass);
                return string.Equals(name, AutoMapAttributeFullName, StringComparison.Ordinal);
            });

        if (autoMapAttr is null)
            return null;

        // Extract SourceType from constructor arg
        if (autoMapAttr.ConstructorArguments.Length < 1)
            return null;

        var sourceTypeArg = autoMapAttr.ConstructorArguments[0];
        if (sourceTypeArg.Value is not INamedTypeSymbol sourceType)
            return null;

        var sourceFullName = GetFullTypeName(sourceType);
        var destFullName = GetFullTypeName(classSymbol);

        var pairs = new List<TypePairReference> { new TypePairReference(sourceFullName, destFullName) };

        // Check for ReverseMap named argument
        foreach (var namedArg in autoMapAttr.NamedArguments)
        {
            if (string.Equals(namedArg.Key, "ReverseMap", StringComparison.Ordinal)
                && namedArg.Value.Value is true)
            {
                pairs.Add(new TypePairReference(destFullName, sourceFullName));
            }
        }

        var className = classSymbol.Name;
        var namespaceName = GetFullNamespace(classSymbol);
        var location = classDecl.GetLocation();
        var filePath = location.SourceTree?.FilePath ?? string.Empty;
        var line = location.GetLineSpan().StartLinePosition.Line;

        return new ProfileInfo(
            $"AutoMap_{className}",
            namespaceName,
            new EquatableArray<TypePairReference>(pairs.ToImmutableArray()),
            filePath,
            line);
    }

    private static bool InheritsFromProfile(INamedTypeSymbol classSymbol)
    {
        var current = classSymbol.BaseType;
        while (current is not null)
        {
            var fullName = GetFullTypeName(current);
            if (string.Equals(fullName, ProfileFullName, StringComparison.Ordinal))
                return true;

            current = current.BaseType;
        }

        return false;
    }

    private static EquatableArray<TypePairReference> ExtractCreateMapCalls(
        ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var pairs = new List<TypePairReference>();
        // Track CreateMap invocations we've already processed (to avoid duplicates when walking chains)
        var processedInvocations = new HashSet<InvocationExpressionSyntax>();

        // Look for CreateMap calls in constructor bodies and method bodies (Configure override)
        var invocations = classDecl.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            ct.ThrowIfCancellationRequested();

            if (processedInvocations.Contains(invocation))
                continue;

            // Check if this is a CreateMap<TSource, TDest>() or CreateProjection<TSource, TDest>() call
            string? methodName = null;

            if (invocation.Expression is GenericNameSyntax genericName)
            {
                methodName = genericName.Identifier.Text;
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name is GenericNameSyntax genericMember)
            {
                methodName = genericMember.Identifier.Text;
            }

            bool isProjection = false;
            if (string.Equals(methodName, "CreateMap", StringComparison.Ordinal))
            {
                isProjection = false;
            }
            else if (string.Equals(methodName, "CreateProjection", StringComparison.Ordinal))
            {
                isProjection = true;
            }
            else
            {
                continue;
            }

            processedInvocations.Add(invocation);

            // Get the method symbol to extract type arguments
            var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol is null || methodSymbol.TypeArguments.Length != 2)
                continue;

            var sourceType = methodSymbol.TypeArguments[0];
            var destType = methodSymbol.TypeArguments[1];

            var sourceFullName = GetFullTypeName(sourceType);
            var destFullName = GetFullTypeName(destType);

            if (string.IsNullOrEmpty(sourceFullName) || string.IsNullOrEmpty(destFullName))
                continue;

            // Extract mapping name from CreateMap<S,D>("name") if present
            string? mappingName = null;
            var createMapArgs = invocation.ArgumentList.Arguments;
            if (createMapArgs.Count >= 1
                && createMapArgs[0].Expression is LiteralExpressionSyntax nameLiteral
                && nameLiteral.Token.Value is string nameValue)
            {
                mappingName = nameValue;
            }

            // Walk the fluent chain to extract ForMember/Ignore/MapFrom/ReverseMap/Include/IncludeBase/MaxDepth/ForCtorParam
            var memberConfigs = new List<MemberConfigReference>();
            bool hasReverseMap = false;
            var includedDerived = new List<IncludedTypeReference>();
            var includedBase = new List<IncludedTypeReference>();
            var ctorParamConfigs = new List<CtorParamConfigReference>();
            int? maxDepth = null;
            string? beforeMapExpr = null;
            string? afterMapExpr = null;
            string? constructUsingExpr = null;
            string? convertUsingExpr = null;
            MemberConfigReference? forAllMembersConfig = null;
            var includedMemberNames = new List<string>();
            bool isDeepClone = false;
            var additionalSourceTypes = new List<string>();

            WalkFluentChain(invocation, semanticModel, memberConfigs, ref hasReverseMap,
                processedInvocations, includedDerived, includedBase, ref maxDepth,
                ref beforeMapExpr, ref afterMapExpr, ref constructUsingExpr,
                ref convertUsingExpr, isProjection, ctorParamConfigs,
                ref forAllMembersConfig, includedMemberNames, ref isDeepClone,
                additionalSourceTypes, ct);

            var memberConfigsArray = new EquatableArray<MemberConfigReference>(memberConfigs.ToImmutableArray());
            var includedDerivedArray = new EquatableArray<IncludedTypeReference>(includedDerived.ToImmutableArray());
            var includedBaseArray = new EquatableArray<IncludedTypeReference>(includedBase.ToImmutableArray());
            var ctorParamConfigsArray = new EquatableArray<CtorParamConfigReference>(ctorParamConfigs.ToImmutableArray());
            var includedMemberNamesArray = new EquatableArray<string>(includedMemberNames.ToImmutableArray());
            var additionalSourceTypesArray = new EquatableArray<string>(additionalSourceTypes.ToImmutableArray());

            pairs.Add(new TypePairReference(sourceFullName, destFullName, memberConfigsArray,
                includedDerivedArray, includedBaseArray, maxDepth, isProjection,
                beforeMapExpr, afterMapExpr, constructUsingExpr, convertUsingExpr,
                ctorParamConfigsArray, forAllMembersConfig, includedMemberNamesArray,
                null, isDeepClone, additionalSourceTypesArray, mappingName));

            if (hasReverseMap && !isProjection)
            {
                pairs.Add(new TypePairReference(destFullName, sourceFullName));
            }
        }

        return new EquatableArray<TypePairReference>(pairs.ToImmutableArray());
    }

    /// <summary>
    /// Walks the fluent method chain starting from the CreateMap invocation,
    /// extracting ForMember/Ignore/MapFrom/ReverseMap/Include/IncludeBase/MaxDepth/
    /// Condition/BeforeMap/AfterMap/ConstructUsing calls.
    /// </summary>
    private static void WalkFluentChain(
        InvocationExpressionSyntax createMapInvocation,
        SemanticModel semanticModel,
        List<MemberConfigReference> memberConfigs,
        ref bool hasReverseMap,
        HashSet<InvocationExpressionSyntax> processedInvocations,
        List<IncludedTypeReference> includedDerived,
        List<IncludedTypeReference> includedBase,
        ref int? maxDepth,
        ref string? beforeMapExpr,
        ref string? afterMapExpr,
        ref string? constructUsingExpr,
        ref string? convertUsingExpr,
        bool isProjection,
        List<CtorParamConfigReference> ctorParamConfigs,
        ref MemberConfigReference? forAllMembersConfig,
        List<string> includedMemberNames,
        ref bool isDeepClone,
        List<string> additionalSourceTypes,
        CancellationToken ct)
    {
        SyntaxNode current = createMapInvocation;

        while (true)
        {
            if (current.Parent is not MemberAccessExpressionSyntax memberAccess)
                break;

            if (memberAccess.Parent is not InvocationExpressionSyntax parentInvocation)
                break;

            processedInvocations.Add(parentInvocation);

            var chainMethodName = memberAccess.Name.Identifier.Text;

            if (string.Equals(chainMethodName, "ForMember", StringComparison.Ordinal))
            {
                ParseForMember(parentInvocation, semanticModel, memberConfigs, ct);
            }
            else if (string.Equals(chainMethodName, "ForPath", StringComparison.Ordinal))
            {
                ParseForPath(parentInvocation, memberConfigs);
            }
            else if (string.Equals(chainMethodName, "Ignore", StringComparison.Ordinal))
            {
                ParseIgnore(parentInvocation, memberConfigs);
            }
            else if (string.Equals(chainMethodName, "MapFrom", StringComparison.Ordinal))
            {
                ParseMapFrom(parentInvocation, memberConfigs);
            }
            else if (string.Equals(chainMethodName, "ReverseMap", StringComparison.Ordinal) && !isProjection)
            {
                hasReverseMap = true;
            }
            else if (string.Equals(chainMethodName, "Include", StringComparison.Ordinal) && !isProjection)
            {
                ParseInclude(parentInvocation, semanticModel, includedDerived, ct);
            }
            else if (string.Equals(chainMethodName, "IncludeBase", StringComparison.Ordinal) && !isProjection)
            {
                ParseInclude(parentInvocation, semanticModel, includedBase, ct);
            }
            else if (string.Equals(chainMethodName, "MaxDepth", StringComparison.Ordinal) && !isProjection)
            {
                ParseMaxDepth(parentInvocation, ref maxDepth);
            }
            else if (string.Equals(chainMethodName, "BeforeMap", StringComparison.Ordinal) && !isProjection)
            {
                ParseLambdaExpression(parentInvocation, ref beforeMapExpr);
            }
            else if (string.Equals(chainMethodName, "AfterMap", StringComparison.Ordinal) && !isProjection)
            {
                ParseLambdaExpression(parentInvocation, ref afterMapExpr);
            }
            else if (string.Equals(chainMethodName, "ConstructUsing", StringComparison.Ordinal) && !isProjection)
            {
                ParseLambdaExpression(parentInvocation, ref constructUsingExpr);
            }
            else if (string.Equals(chainMethodName, "ConvertUsing", StringComparison.Ordinal) && !isProjection)
            {
                ParseLambdaExpression(parentInvocation, ref convertUsingExpr);
            }
            else if (string.Equals(chainMethodName, "ForCtorParam", StringComparison.Ordinal) && !isProjection)
            {
                ParseForCtorParam(parentInvocation, ctorParamConfigs);
            }
            else if (string.Equals(chainMethodName, "ForAllMembers", StringComparison.Ordinal))
            {
                ParseForAllMembers(parentInvocation, ref forAllMembersConfig);
            }
            else if (string.Equals(chainMethodName, "IncludeMembers", StringComparison.Ordinal))
            {
                ParseIncludeMembers(parentInvocation, includedMemberNames);
            }
            else if (string.Equals(chainMethodName, "UseDeepCloning", StringComparison.Ordinal) && !isProjection)
            {
                isDeepClone = true;
            }
            else if (string.Equals(chainMethodName, "IncludeSource", StringComparison.Ordinal) && !isProjection)
            {
                ParseIncludeSource(parentInvocation, semanticModel, additionalSourceTypes, ct);
            }
            // Other methods (NullSubstitute on mapping level, etc.) — skip silently

            current = parentInvocation;
        }
    }

    /// <summary>
    /// Parses .Include&lt;TDerivedSource, TDerivedDest&gt;() or .IncludeBase&lt;TBaseSource, TBaseDest&gt;()
    /// </summary>
    private static void ParseInclude(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        List<IncludedTypeReference> targetList,
        CancellationToken ct)
    {
        // Resolve via semantic model to get type arguments
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (methodSymbol is null || methodSymbol.TypeArguments.Length != 2)
            return;

        var srcFullName = GetFullTypeName(methodSymbol.TypeArguments[0]);
        var destFullName = GetFullTypeName(methodSymbol.TypeArguments[1]);

        if (!string.IsNullOrEmpty(srcFullName) && !string.IsNullOrEmpty(destFullName))
        {
            targetList.Add(new IncludedTypeReference(srcFullName, destFullName));
        }
    }

    /// <summary>
    /// Parses .MaxDepth(N) from the fluent chain.
    /// </summary>
    private static void ParseMaxDepth(InvocationExpressionSyntax invocation, ref int? maxDepth)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 1)
            return;

        var argExpr = args[0].Expression;
        if (argExpr is LiteralExpressionSyntax literal && literal.Token.Value is int value)
        {
            maxDepth = value;
        }
    }

    /// <summary>
    /// Parses a lambda expression argument (first arg) and stores the lambda source text.
    /// Used for BeforeMap, AfterMap, ConstructUsing.
    /// </summary>
    private static void ParseLambdaExpression(InvocationExpressionSyntax invocation, ref string? target)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 1)
            return;

        var argExpr = args[0].Expression;
        target = argExpr.ToFullString().Trim();
    }

    /// <summary>
    /// Parses .ForCtorParam("paramName", opt => opt.MapFrom(s => s.Prop))
    /// </summary>
    private static void ParseForCtorParam(
        InvocationExpressionSyntax invocation,
        List<CtorParamConfigReference> ctorParamConfigs)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
            return;

        // First arg: string literal with param name
        string? paramName = null;
        if (args[0].Expression is LiteralExpressionSyntax literal && literal.Token.Value is string strValue)
        {
            paramName = strValue;
        }

        if (paramName is null)
            return;

        // Second arg: opt => opt.MapFrom(s => s.Prop)
        var optionsExpr = args[1].Expression;
        if (optionsExpr is not SimpleLambdaExpressionSyntax optionsLambda)
            return;

        InvocationExpressionSyntax? optInvocation = null;
        if (optionsLambda.Body is InvocationExpressionSyntax inv)
            optInvocation = inv;
        else if (optionsLambda.Body is ExpressionStatementSyntax exprStmt
            && exprStmt.Expression is InvocationExpressionSyntax inv2)
            optInvocation = inv2;

        if (optInvocation is null)
            return;

        string? optMethodName = null;
        if (optInvocation.Expression is MemberAccessExpressionSyntax optMemberAccess)
            optMethodName = optMemberAccess.Name.Identifier.Text;

        if (string.Equals(optMethodName, "MapFrom", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var sourceName = ExtractMemberName(optArgs[0].Expression);
                ctorParamConfigs.Add(new CtorParamConfigReference(paramName, sourceName));
            }
        }
    }

    /// <summary>
    /// Parses .ForMember(d => d.Prop, opt => opt.MapFrom(s => s.Other)) or opt => opt.Ignore()
    /// Also handles opt => opt.Condition(...) and opt => opt.PreCondition(...)
    /// </summary>
    private static void ParseForMember(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        List<MemberConfigReference> memberConfigs,
        CancellationToken ct)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
            return;

        var destName = ExtractMemberName(args[0].Expression);
        if (destName is null)
            return;

        // Second arg: opt => opt.MapFrom(s => s.X) or opt => opt.Ignore()
        var optionsExpr = args[1].Expression;
        if (optionsExpr is not SimpleLambdaExpressionSyntax optionsLambda)
            return;

        // The body should be an InvocationExpression like opt.MapFrom(s => s.X) or opt.Ignore()
        InvocationExpressionSyntax? optInvocation = null;
        if (optionsLambda.Body is InvocationExpressionSyntax inv)
        {
            optInvocation = inv;
        }
        else if (optionsLambda.Body is ExpressionStatementSyntax exprStmt
            && exprStmt.Expression is InvocationExpressionSyntax inv2)
        {
            optInvocation = inv2;
        }

        if (optInvocation is null)
            return;

        string? optMethodName = null;
        if (optInvocation.Expression is MemberAccessExpressionSyntax optMemberAccess)
        {
            optMethodName = optMemberAccess.Name.Identifier.Text;
        }

        if (optMethodName is null)
            return;

        if (string.Equals(optMethodName, "Ignore", StringComparison.Ordinal))
        {
            memberConfigs.Add(new MemberConfigReference(destName, null, true));
        }
        else if (string.Equals(optMethodName, "MapFrom", StringComparison.Ordinal))
        {
            // Check for generic type args first (resolver-based MapFrom)
            if (optInvocation.Expression is MemberAccessExpressionSyntax resolverAccess
                && resolverAccess.Name is GenericNameSyntax genericResolverName)
            {
                var genericArgCount = genericResolverName.TypeArgumentList.Arguments.Count;

                if (genericArgCount == 2)
                {
                    // opt.MapFrom<TResolver, TSourceMember>(s => s.Prop) — IMemberValueResolver
                    var resolverSymbolInfo = semanticModel.GetSymbolInfo(optInvocation, ct);
                    var resolverMethodSymbol = resolverSymbolInfo.Symbol as IMethodSymbol
                        ?? resolverSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

                    if (resolverMethodSymbol is not null && resolverMethodSymbol.TypeArguments.Length == 2)
                    {
                        var resolverTypeName = GetFullTypeName(resolverMethodSymbol.TypeArguments[0]);
                        string? sourceMemberName = null;

                        // Extract source member from lambda argument
                        var mapFromArgs = optInvocation.ArgumentList.Arguments;
                        if (mapFromArgs.Count >= 1)
                        {
                            sourceMemberName = ExtractMemberName(mapFromArgs[0].Expression);
                        }

                        if (!string.IsNullOrEmpty(resolverTypeName))
                        {
                            memberConfigs.Add(new MemberConfigReference(
                                destName, sourceMemberName, false, null, null, null, null, resolverTypeName));
                        }
                    }
                }
                else if (genericArgCount == 1)
                {
                    // opt.MapFrom<TResolver>() — IValueResolver
                    var resolverSymbolInfo = semanticModel.GetSymbolInfo(optInvocation, ct);
                    var resolverMethodSymbol = resolverSymbolInfo.Symbol as IMethodSymbol
                        ?? resolverSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

                    if (resolverMethodSymbol is not null && resolverMethodSymbol.TypeArguments.Length == 1)
                    {
                        var resolverTypeName = GetFullTypeName(resolverMethodSymbol.TypeArguments[0]);
                        if (!string.IsNullOrEmpty(resolverTypeName))
                        {
                            memberConfigs.Add(new MemberConfigReference(
                                destName, null, false, null, null, null, resolverTypeName));
                        }
                    }
                }
            }
            else
            {
                // Expression-based MapFrom: opt.MapFrom(s => s.Prop)
                var optArgs = optInvocation.ArgumentList.Arguments;
                if (optArgs.Count >= 1)
                {
                    var sourceName = ExtractMemberName(optArgs[0].Expression);
                    if (sourceName is not null)
                    {
                        memberConfigs.Add(new MemberConfigReference(destName, sourceName, false));
                    }
                }
            }
        }
        else if (string.Equals(optMethodName, "NullSubstitute", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var nullSubText = optArgs[0].Expression.ToFullString().Trim();
                var existing = memberConfigs.FindIndex(c =>
                    string.Equals(c.DestMemberName, destName, StringComparison.Ordinal));
                if (existing >= 0)
                {
                    var old = memberConfigs[existing];
                    memberConfigs[existing] = new MemberConfigReference(
                        old.DestMemberName, old.SourceMemberName, old.IsIgnored,
                        old.ConditionExpression, old.PreConditionExpression,
                        nullSubText, old.ValueResolverTypeName);
                }
                else
                {
                    memberConfigs.Add(new MemberConfigReference(
                        destName, null, false, null, null, nullSubText, null));
                }
            }
        }
        else if (string.Equals(optMethodName, "Condition", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var conditionText = ExtractLambdaBodyText(optArgs[0].Expression);
                if (conditionText is not null)
                {
                    // Check if there's already a config for this member (from a chained call)
                    var existing = memberConfigs.FindIndex(c =>
                        string.Equals(c.DestMemberName, destName, StringComparison.Ordinal));
                    if (existing >= 0)
                    {
                        var old = memberConfigs[existing];
                        memberConfigs[existing] = new MemberConfigReference(
                            old.DestMemberName, old.SourceMemberName, old.IsIgnored,
                            conditionText, old.PreConditionExpression);
                    }
                    else
                    {
                        memberConfigs.Add(new MemberConfigReference(destName, null, false, conditionText, null));
                    }
                }
            }
        }
        else if (string.Equals(optMethodName, "PreCondition", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var conditionText = ExtractLambdaBodyText(optArgs[0].Expression);
                if (conditionText is not null)
                {
                    var existing = memberConfigs.FindIndex(c =>
                        string.Equals(c.DestMemberName, destName, StringComparison.Ordinal));
                    if (existing >= 0)
                    {
                        var old = memberConfigs[existing];
                        memberConfigs[existing] = new MemberConfigReference(
                            old.DestMemberName, old.SourceMemberName, old.IsIgnored,
                            old.ConditionExpression, conditionText);
                    }
                    else
                    {
                        memberConfigs.Add(new MemberConfigReference(destName, null, false, null, conditionText));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Parses .Ignore(d => d.Prop)
    /// </summary>
    private static void ParseIgnore(
        InvocationExpressionSyntax invocation,
        List<MemberConfigReference> memberConfigs)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 1)
            return;

        var destName = ExtractMemberName(args[0].Expression);
        if (destName is not null)
        {
            memberConfigs.Add(new MemberConfigReference(destName, null, true));
        }
    }

    /// <summary>
    /// Parses .MapFrom(d => d.Prop, s => s.Other)
    /// </summary>
    private static void ParseMapFrom(
        InvocationExpressionSyntax invocation,
        List<MemberConfigReference> memberConfigs)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
            return;

        var destName = ExtractMemberName(args[0].Expression);
        var sourceName = ExtractMemberName(args[1].Expression);

        if (destName is not null && sourceName is not null)
        {
            memberConfigs.Add(new MemberConfigReference(destName, sourceName, false));
        }
    }

    /// <summary>
    /// Extracts a simple member name from a lambda expression like d => d.PropertyName.
    /// Returns null if the expression is not a simple member access.
    /// </summary>
    private static string? ExtractMemberName(ExpressionSyntax expression)
    {
        // Handle: d => d.Prop
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
        {
            return ExtractMemberNameFromBody(simpleLambda.Body);
        }

        // Handle: (d) => d.Prop
        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            return ExtractMemberNameFromBody(parenLambda.Body);
        }

        return null;
    }

    /// <summary>
    /// Extracts a member name from a lambda body, unwrapping cast expressions if present.
    /// Handles: d.Prop, (object)d.Prop, (SomeType)d.Prop
    /// </summary>
    private static string? ExtractMemberNameFromBody(CSharpSyntaxNode body)
    {
        // Direct member access: d.Prop
        if (body is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.Text;
        }

        // Cast expression: (object)d.Prop or (SomeType)d.Prop
        if (body is CastExpressionSyntax castExpr)
        {
            if (castExpr.Expression is MemberAccessExpressionSyntax castMemberAccess)
            {
                return castMemberAccess.Name.Identifier.Text;
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the body text from a lambda expression.
    /// For (s, d) => s.IsActive, returns "s.IsActive".
    /// For s => s.IsActive, returns "s.IsActive".
    /// </summary>
    private static string? ExtractLambdaBodyText(ExpressionSyntax expression)
    {
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
        {
            return simpleLambda.Body.ToFullString().Trim();
        }

        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            return parenLambda.Body.ToFullString().Trim();
        }

        return null;
    }

    /// <summary>
    /// Extracts RecognizePrefixes/RecognizePostfixes string literal arguments from the profile class.
    /// </summary>
    private static (EquatableArray<string> prefixes, EquatableArray<string> postfixes) ExtractPrefixesAndPostfixes(
        ClassDeclarationSyntax classDecl)
    {
        var prefixes = new List<string>();
        var postfixes = new List<string>();

        var invocations = classDecl.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            string? methodName = null;

            if (invocation.Expression is IdentifierNameSyntax identName)
            {
                methodName = identName.Identifier.Text;
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name is IdentifierNameSyntax memberIdentName)
            {
                methodName = memberIdentName.Identifier.Text;
            }

            if (methodName is null)
                continue;

            List<string>? targetList = null;
            if (string.Equals(methodName, "RecognizePrefixes", StringComparison.Ordinal))
                targetList = prefixes;
            else if (string.Equals(methodName, "RecognizePostfixes", StringComparison.Ordinal))
                targetList = postfixes;

            if (targetList is null)
                continue;

            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (arg.Expression is LiteralExpressionSyntax literal
                    && literal.Token.Value is string strValue)
                {
                    targetList.Add(strValue);
                }
            }
        }

        return (
            new EquatableArray<string>(prefixes.ToImmutableArray()),
            new EquatableArray<string>(postfixes.ToImmutableArray()));
    }

    /// <summary>
    /// Parses .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City)) or opt.Ignore()
    /// </summary>
    private static void ParseForPath(
        InvocationExpressionSyntax invocation,
        List<MemberConfigReference> memberConfigs)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
            return;

        // First arg: d => d.Address.City — extract full dotted path
        var destPath = ExtractMemberPath(args[0].Expression);
        if (destPath is null)
            return;

        // Second arg: opt => opt.MapFrom(s => s.X) or opt => opt.Ignore()
        var optionsExpr = args[1].Expression;
        if (optionsExpr is not SimpleLambdaExpressionSyntax optionsLambda)
            return;

        InvocationExpressionSyntax? optInvocation = null;
        if (optionsLambda.Body is InvocationExpressionSyntax inv)
            optInvocation = inv;
        else if (optionsLambda.Body is ExpressionStatementSyntax exprStmt
            && exprStmt.Expression is InvocationExpressionSyntax inv2)
            optInvocation = inv2;

        if (optInvocation is null)
            return;

        string? optMethodName = null;
        if (optInvocation.Expression is MemberAccessExpressionSyntax optMemberAccess)
            optMethodName = optMemberAccess.Name.Identifier.Text;

        if (optMethodName is null)
            return;

        if (string.Equals(optMethodName, "Ignore", StringComparison.Ordinal))
        {
            memberConfigs.Add(new MemberConfigReference(destPath, null, true));
        }
        else if (string.Equals(optMethodName, "MapFrom", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var sourceName = ExtractMemberName(optArgs[0].Expression);
                if (sourceName is not null)
                {
                    memberConfigs.Add(new MemberConfigReference(destPath, sourceName, false));
                }
            }
        }
    }

    /// <summary>
    /// Walks a MemberAccessExpression chain to build a dotted path.
    /// E.g., d => d.Address.City → "Address.City"
    /// </summary>
    private static string? ExtractMemberPath(ExpressionSyntax expression)
    {
        CSharpSyntaxNode? body = null;

        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
            body = simpleLambda.Body;
        else if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
            body = parenLambda.Body;

        if (body is not MemberAccessExpressionSyntax memberAccess)
            return null;

        // Walk the chain from right to left
        var segments = new List<string>();
        var current = memberAccess;

        while (current is not null)
        {
            segments.Add(current.Name.Identifier.Text);

            if (current.Expression is MemberAccessExpressionSyntax parent)
            {
                current = parent;
            }
            else
            {
                // We've reached the parameter (d), stop
                break;
            }
        }

        if (segments.Count < 2)
            return null; // Not a deep path, just a simple member

        segments.Reverse();
        return string.Join(".", segments);
    }

    /// <summary>
    /// Parses .ForAllMembers(opt => opt.Ignore()) or similar member config.
    /// </summary>
    private static void ParseForAllMembers(
        InvocationExpressionSyntax invocation,
        ref MemberConfigReference? forAllMembersConfig)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 1)
            return;

        var optionsExpr = args[0].Expression;
        if (optionsExpr is not SimpleLambdaExpressionSyntax optionsLambda)
            return;

        InvocationExpressionSyntax? optInvocation = null;
        if (optionsLambda.Body is InvocationExpressionSyntax inv)
            optInvocation = inv;
        else if (optionsLambda.Body is ExpressionStatementSyntax exprStmt
            && exprStmt.Expression is InvocationExpressionSyntax inv2)
            optInvocation = inv2;

        if (optInvocation is null)
            return;

        string? optMethodName = null;
        if (optInvocation.Expression is MemberAccessExpressionSyntax optMemberAccess)
            optMethodName = optMemberAccess.Name.Identifier.Text;

        if (optMethodName is null)
            return;

        if (string.Equals(optMethodName, "Ignore", StringComparison.Ordinal))
        {
            forAllMembersConfig = new MemberConfigReference("*", null, true);
        }
        else if (string.Equals(optMethodName, "Condition", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var conditionText = ExtractLambdaBodyText(optArgs[0].Expression);
                if (conditionText is not null)
                {
                    forAllMembersConfig = new MemberConfigReference("*", null, false, conditionText, null);
                }
            }
        }
        else if (string.Equals(optMethodName, "PreCondition", StringComparison.Ordinal))
        {
            var optArgs = optInvocation.ArgumentList.Arguments;
            if (optArgs.Count >= 1)
            {
                var conditionText = ExtractLambdaBodyText(optArgs[0].Expression);
                if (conditionText is not null)
                {
                    forAllMembersConfig = new MemberConfigReference("*", null, false, null, conditionText);
                }
            }
        }
    }

    /// <summary>
    /// Parses .IncludeMembers(s => s.Nav1, s => s.Nav2) to extract member names.
    /// </summary>
    private static void ParseIncludeMembers(
        InvocationExpressionSyntax invocation,
        List<string> includedMemberNames)
    {
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            var memberName = ExtractMemberName(arg.Expression);
            if (memberName is not null)
            {
                includedMemberNames.Add(memberName);
            }
        }
    }

    /// <summary>
    /// Parses .IncludeSource&lt;TOther&gt;() to extract the additional source type.
    /// </summary>
    private static void ParseIncludeSource(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        List<string> additionalSourceTypes,
        CancellationToken ct)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (methodSymbol is null || methodSymbol.TypeArguments.Length != 1)
            return;

        var typeName = GetFullTypeName(methodSymbol.TypeArguments[0]);
        if (!string.IsNullOrEmpty(typeName))
        {
            additionalSourceTypes.Add(typeName);
        }
    }

    /// <summary>
    /// Detects AllowNullCollections = true assignment in a profile constructor or field initializer.
    /// </summary>
    private static bool DetectAllowNullCollections(ClassDeclarationSyntax classDecl)
    {
        var assignments = classDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>();
        foreach (var assignment in assignments)
        {
            if (assignment.Left is IdentifierNameSyntax identifier
                && string.Equals(identifier.Identifier.Text, "AllowNullCollections", StringComparison.Ordinal)
                && assignment.Right is LiteralExpressionSyntax literal
                && literal.Token.Value is true)
            {
                return true;
            }
        }
        return false;
    }

    private static string GetFullTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
    }

    private static string GetFullNamespace(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace;
        if (ns is null || ns.IsGlobalNamespace)
            return string.Empty;

        return ns.ToDisplayString();
    }
}
