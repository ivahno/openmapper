using Microsoft.CodeAnalysis;

namespace OpenAutoMapper.Generator.Diagnostics;

internal static class DiagnosticDescriptors
{
    // Category
    private const string Category = "OpenAutoMapper";

    public static readonly DiagnosticDescriptor SourceTypeUnknown = new(
        "OM1001", "Source type unknown",
        "Cannot generate mapping for '{0}' -> '{1}'. Source type is not statically known. Use Map<TSource, TDest>(source) with concrete type.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor DestTypeUnknown = new(
        "OM1002", "Destination type unknown",
        "Cannot generate mapping for '{0}' -> '{1}'. Destination type is not statically known.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor OpenGenericNotSupported = new(
        "OM1003", "Open generic mapping not supported",
        "CreateMap with open generic types is not supported under AOT. Register closed types: CreateMap<{0}, {1}>().",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InterfaceTargetNotSupported = new(
        "OM1004", "Interface target not supported",
        "Cannot generate mapping to interface '{0}'. Map to a concrete type instead.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor UnmappedDestProperty = new(
        "OM1010", "Unmapped destination property",
        "Property '{0}' on '{1}' is not mapped from '{2}'. Either map it explicitly or add [IgnoreMap].",
        Category, DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor SensitivePropertyMapped = new(
        "OM1011", "Sensitive property mapped",
        "Property '{0}' on '{1}' is marked [SensitiveProperty] and must be explicitly configured",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidMaxDepth = new(
        "OM1020", "Invalid MaxDepth value",
        "MaxDepth value must be a positive integer. Found '{0}' on mapping '{1}' -> '{2}'.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor IncludeTypeMismatch = new(
        "OM1021", "Include type mismatch",
        "Include<{0}, {1}>() requires {0} to derive from {2}. The type does not inherit from the base source type.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor IncludeBaseTypeMismatch = new(
        "OM1022", "IncludeBase type mismatch",
        "IncludeBase<{0}, {1}>() on mapping '{2}' to '{3}' has no matching base mapping found",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor CircularReferenceDetected = new(
        "OM1030", "Circular reference detected",
        "Circular reference detected in mapping '{0}' -> '{1}'. Depth tracking will be emitted (MaxDepth={2}).",
        Category, DiagnosticSeverity.Info, true);

    public static readonly DiagnosticDescriptor CircularReferenceInProjection = new(
        "OM1040", "Circular reference in projection",
        "Circular reference detected in projection '{0}' -> '{1}'. Expression tree projections cannot handle circular references.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor PolymorphicIncludeInProjection = new(
        "OM1041", "Polymorphic Include in projection",
        "Include/IncludeBase is not supported in CreateProjection. Projection '{0}' -> '{1}' cannot use polymorphic dispatch.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor DictionaryPropertyInProjection = new(
        "OM1042", "Dictionary property in projection",
        "Dictionary property '{0}' on projection '{1}' -> '{2}' is not translatable to SQL. Property will be skipped.",
        Category, DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor NoMatchingConstructor = new(
        "OM1050", "No matching constructor",
        "Destination type '{0}' has no parameterless constructor. No constructor parameters match source properties on '{1}'.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor CtorParamNotFound = new(
        "OM1051", "Constructor parameter not found",
        "ForCtorParam('{0}') does not match any constructor parameter on '{1}'. Verify the parameter name is correct.",
        Category, DiagnosticSeverity.Warning, true);
}
