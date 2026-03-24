using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void OM1001_SourceTypeNotFound()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Where(d => d.Id == "OM1001").Should().BeEmpty("valid types should not trigger OM1001");
    }

    [Fact]
    public void OM1003_OpenGenericMapping_DescriptorExists()
    {
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");
        var field = descriptorType.GetField("OpenGenericNotSupported",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull();
        var descriptor = (DiagnosticDescriptor)field!.GetValue(null)!;
        descriptor.Id.Should().Be("OM1003");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public void OM1004_InterfaceTargetNotSupported_DescriptorExists()
    {
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");
        var field = descriptorType.GetField("InterfaceTargetNotSupported",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull();
        var descriptor = (DiagnosticDescriptor)field!.GetValue(null)!;
        descriptor.Id.Should().Be("OM1004");
    }

    [Fact]
    public void OM1010_UnmappedProperty_Warning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Extra { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        var unmapped = diagnostics.Where(d => d.Id == "OM1010").ToList();
        unmapped.Should().NotBeEmpty();
        unmapped.First().Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void OM1011_SensitiveProperty_Error()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public string SSN { get; set; } }
public class Dest { [SensitiveProperty] public string SSN { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        var sensitive = diagnostics.Where(d => d.Id == "OM1011").ToList();
        sensitive.Should().NotBeEmpty();
        sensitive.First().Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public void Generator_WithEmptySource_ProducesNoDiagnostics()
    {
        var source = @"
namespace TestApp;
public class EmptyClass { }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        GetOMDiagnostics(diagnostics).Should().BeEmpty();
    }
}
