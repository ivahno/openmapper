using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

/// <summary>
/// Generator snapshot tests split into partial classes by category.
/// See: GeneratorSnapshotTests.PropertyMatching.cs
///      GeneratorSnapshotTests.TypeConversions.cs
///      GeneratorSnapshotTests.Attributes.cs
///      GeneratorSnapshotTests.Diagnostics.cs
///      GeneratorSnapshotTests.MapperImpl.cs
///      GeneratorSnapshotTests.MultipleProfiles.cs
///      GeneratorSnapshotTests.EdgeCases.cs
/// </summary>
public partial class GeneratorSnapshotTests
{
    private static List<Diagnostic> GetOMErrors(IReadOnlyList<Diagnostic> diagnostics)
    {
        return diagnostics
            .Where(d => d.Id.StartsWith("OM", StringComparison.Ordinal)
                     && d.Severity == DiagnosticSeverity.Error)
            .ToList();
    }

    private static List<Diagnostic> GetOMDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        return diagnostics
            .Where(d => d.Id.StartsWith("OM", StringComparison.Ordinal))
            .ToList();
    }
}
