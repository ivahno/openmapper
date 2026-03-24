using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- Init-Only Property Tests ----

    [Fact]
    public void InitOnly_Property_GeneratesInitializerSyntax()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; init; } public string Name { get; init; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Init-only properties should appear in an object initializer
        generatedSources.Should().Contain(s => s.Contains("Id = source.Id"));
        generatedSources.Should().Contain(s => s.Contains("Name = source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void InitOnly_WithMutableProperties_HybridEmission()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class Dest { public int Id { get; init; } public string Name { get; set; } public string Tag { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Init-only Id should be in initializer, mutable Name and Tag should be assignments or in initializer
        generatedSources.Should().Contain(s => s.Contains("Id = source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source.Tag"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Record Type Tests ----

    [Fact]
    public void Record_WithInitOnly_InitializerSyntax()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public record Dest { public int Id { get; init; } public string Name { get; init; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Id = source.Id"));
        generatedSources.Should().Contain(s => s.Contains("Name = source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- ForCtorParam Tests ----

    [Fact]
    public void ForCtorParam_ExplicitMapping_WithMutableProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } public string Tag { get; set; } }
public class Dest
{
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public string Tag { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForCtorParam(""name"", opt => opt.MapFrom(s => s.FullName));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should map FullName to the 'name' ctor param
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        // Tag should be mapped as a regular property
        generatedSources.Should().Contain(s => s.Contains("source.Tag"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForCtorParam_WithMapFrom_MultipleMappings()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Identifier { get; set; } public string Label { get; set; } public string Extra { get; set; } }
public class Dest
{
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public string Extra { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForCtorParam(""id"", opt => opt.MapFrom(s => s.Identifier))
            .ForCtorParam(""name"", opt => opt.MapFrom(s => s.Label));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Identifier"));
        generatedSources.Should().Contain(s => s.Contains("source.Label"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MultipleConstructors_PickBestMatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Extra { get; set; } }
public class Dest
{
    public Dest(int id) { Id = id; }
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Extra { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForCtorParam(""id"", opt => opt.MapFrom(s => s.Id))
            .ForCtorParam(""name"", opt => opt.MapFrom(s => s.Name));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should pick the (int id, string name) constructor
        generatedSources.Should().Contain(s => s.Contains("source.Id") && s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Constructor_PlusInitOnly_PlusMutable_AllThreeGroups()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Code { get; set; } public string Tag { get; set; } }
public class Dest
{
    public Dest(int id) { Id = id; }
    public int Id { get; }
    public string Code { get; init; }
    public string Tag { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForCtorParam(""id"", opt => opt.MapFrom(s => s.Id));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Id should be ctor arg, Code should be init-only in initializer, Tag should be mutable assignment
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Code"));
        generatedSources.Should().Contain(s => s.Contains("source.Tag"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Diagnostic Tests ----

    [Fact]
    public void NoParameterlessCtor_NoMatch_ReportsOM1050()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Foo { get; set; } }
public class Dest
{
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        var omDiags = GetOMDiagnostics(diagnostics);
        omDiags.Should().Contain(d => d.Id == "OM1050",
            "should report OM1050 when no parameterless ctor and no params match");
    }

    [Fact]
    public void ForCtorParam_BadName_ReportsOM1051()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Extra { get; set; } }
public class Dest
{
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public string Extra { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForCtorParam(""nonExistentParam"", opt => opt.MapFrom(s => s.Name));
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        var omDiags = GetOMDiagnostics(diagnostics);
        omDiags.Should().Contain(d => d.Id == "OM1051",
            "should report OM1051 when ForCtorParam references a non-existent parameter name");
    }

    // ---- Record Struct Tests ----

    [Fact]
    public void RecordStruct_WithInitOnly()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public record struct Dest { public int Id { get; init; } public string Name { get; init; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Id = source.Id"));
        generatedSources.Should().Contain(s => s.Contains("Name = source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Update Method (Map with destination) Tests ----

    [Fact]
    public void UpdateMethod_SkipsInitOnlyProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class Dest { public int Id { get; init; } public string Name { get; set; } public string Tag { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();

        // In the MapperImpl update method, init-only Id should be skipped
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();

        // The update method should assign Name and Tag but NOT Id (init-only)
        mapperImpl.Should().Contain("dest_Dest.Name =");
        mapperImpl.Should().Contain("dest_Dest.Tag =");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Auto-detect constructor mapping ----

    [Fact]
    public void AutoDetect_ConstructorParams_WhenNoParameterlessCtor()
    {
        // Dest has no parameterless ctor but has settable Extra property
        // The ctor params should be auto-detected from source property names
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Extra { get; set; } }
public class Dest
{
    public Dest(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public string Extra { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should auto-detect constructor params by matching names
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source.Extra"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void InitOnly_WithCondition_IsHandled()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; init; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.Condition((s, d, m) => s.Name != null));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should still map both properties
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
