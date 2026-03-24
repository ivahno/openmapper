using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void ForMember_MapFrom_RenamesProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        generatedSources.Should().Contain(s => s.Contains("Name = source.FullName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForMember_Ignore_SkipsProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Secret { get; set; } }
public class Dest { public int Id { get; set; } public string Secret { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Secret, opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().NotContain(s => s.Contains("source.Secret"));
        GetOMDiagnostics(diagnostics).Where(d => d.Id == "OM1010").Should().BeEmpty("ignored member should not produce unmapped warning");
    }

    [Fact]
    public void FluentIgnore_TopLevel()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Hidden { get; set; } }
public class Dest { public int Id { get; set; } public string Hidden { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .Ignore(d => d.Hidden);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().NotContain(s => s.Contains("source.Hidden"));
        GetOMDiagnostics(diagnostics).Where(d => d.Id == "OM1010").Should().BeEmpty();
    }

    [Fact]
    public void FluentMapFrom_TopLevel()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FirstName { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .MapFrom(d => d.Name, s => s.FirstName);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.FirstName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ReverseMap_GeneratesBothDirections()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ReverseMap();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        generatedSources.Should().Contain(s => s.Contains("MapToSource"));
    }

    [Fact]
    public void ForMember_SuppressesSensitivePropertyError()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string SSN { get; set; } }
public class Dest { public int Id { get; set; } [SensitiveProperty] public string SSN { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.SSN, opt => opt.MapFrom(s => s.SSN));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.SSN"));
        diagnostics.Where(d => d.Id == "OM1011").Should().BeEmpty("fluent ForMember should suppress sensitive property error");
    }

    [Fact]
    public void ForMember_SuppressesUnmappedWarning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Extra { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Extra, opt => opt.Ignore());
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Where(d => d.Id == "OM1010").Should().BeEmpty("ForMember Ignore should suppress unmapped warning");
    }

    [Fact]
    public void ForMember_ChainedMultiple()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } public string Secret { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Secret { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
            .ForMember(d => d.Secret, opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        generatedSources.Should().NotContain(s => s.Contains("source.Secret"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void FluentConfig_OverridesAttribute()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Alt { get; set; } public string Original { get; set; } }
public class Dest { public int Id { get; set; } [MapFrom(""Original"")] public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Alt));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Fluent config should override the attribute
        generatedSources.Should().Contain(s => s.Contains("source.Alt"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForMember_IgnoredProperty_NoUnmappedWarning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Description { get; set; } public string Notes { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .Ignore(d => d.Description)
            .Ignore(d => d.Notes);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Id == "OM1010").Should().BeEmpty("all unmapped properties are ignored via fluent API");
    }
}
