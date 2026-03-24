using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public class DiagnosticTests
{
    private static Assembly? LoadGeneratorAssembly()
    {
        var assemblyName = typeof(DiagnosticTests).Assembly
            .GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "OpenAutoMapper.Generator");

        if (assemblyName is not null)
            return Assembly.Load(assemblyName);

        try { return Assembly.Load("OpenAutoMapper.Generator"); }
        catch (System.IO.FileNotFoundException) { return null; }
    }

    [Fact]
    public void GeneratorAssembly_CanBeLoaded()
    {
        var assembly = LoadGeneratorAssembly();
        assembly.Should().NotBeNull(
            "the test project should be able to load the OpenAutoMapper.Generator assembly");
    }

    [Fact]
    public void DiagnosticDescriptors_AreDefinedCorrectly()
    {
        var assembly = LoadGeneratorAssembly();
        if (assembly is null) return;

        var descriptors = assembly.GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => (Field: f, Descriptor: (DiagnosticDescriptor)f.GetValue(null)!))
            .ToList();

        foreach (var (field, descriptor) in descriptors)
        {
            descriptor.Id.Should().StartWith("OM",
                $"diagnostic descriptor '{field.Name}' should use the OM prefix");
            descriptor.Title.ToString(System.Globalization.CultureInfo.InvariantCulture)
                .Should().NotBeNullOrWhiteSpace($"diagnostic descriptor '{field.Name}' should have a title");
            descriptor.Category.Should().NotBeNullOrWhiteSpace(
                $"diagnostic descriptor '{field.Name}' should have a category");
            descriptor.DefaultSeverity.Should().BeOneOf(
                DiagnosticSeverity.Error, DiagnosticSeverity.Warning,
                DiagnosticSeverity.Info, DiagnosticSeverity.Hidden);
        }
    }

    [Fact]
    public void DiagnosticDescriptors_HaveUniqueIds()
    {
        var assembly = LoadGeneratorAssembly();
        if (assembly is null) return;

        var descriptorIds = assembly.GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => ((DiagnosticDescriptor)f.GetValue(null)!).Id)
            .ToList();

        descriptorIds.Should().OnlyHaveUniqueItems(
            "each diagnostic descriptor should have a unique ID");
    }

    [Fact]
    public void OM1010_EmittedForUnmappedProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Missing { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1010" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void OM1011_EmittedForSensitiveProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public string SSN { get; set; } }
public class Dest { [SensitiveProperty] public string SSN { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1011" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NoDiagnostics_ForPerfectMatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Where(d => d.Id.StartsWith("OM", StringComparison.Ordinal)).Should().BeEmpty();
    }

    [Fact]
    public void MultipleUnmapped_EmitsMultipleDiagnostics()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string A { get; set; } public string B { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Where(d => d.Id == "OM1010").Should().HaveCount(2);
    }

    [Fact]
    public void DiagnosticDescriptors_HaveExpectedCount()
    {
        var assembly = LoadGeneratorAssembly();
        if (assembly is null) return;

        var descriptors = assembly.GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .ToList();

        descriptors.Should().HaveCountGreaterThanOrEqualTo(6,
            "there should be at least 6 diagnostic descriptors defined");
    }
}
