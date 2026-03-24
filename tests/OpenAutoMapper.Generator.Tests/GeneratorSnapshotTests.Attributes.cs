using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void IgnoreMapAttribute_SkipsProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Secret { get; set; } }
public class Dest { public int Id { get; set; } [IgnoreMap] public string Secret { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().NotContain(s => s.Contains("source.Secret"));
    }

    [Fact]
    public void IgnoreAttribute_SkipsProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Internal { get; set; } }
public class Dest { public int Id { get; set; } [Ignore] public string Internal { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().NotContain(s => s.Contains("source.Internal"));
    }

    [Fact]
    public void MapFromAttribute_RenamesProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } }
public class Dest { public int Id { get; set; } [MapFrom(""FullName"")] public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
    }

    [Fact]
    public void SensitivePropertyAttribute_ProducesError()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string SSN { get; set; } }
public class Dest { public int Id { get; set; } [SensitiveProperty] public string SSN { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1011");
    }

    [Fact]
    public void AutoMapAttribute_ClassLevel()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
[AutoMap(typeof(Source))]
public class Dest { public int Id { get; set; } public string Name { get; set; } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AutoMapAttribute_WithReverseMap()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
[AutoMap(typeof(Source), ReverseMap = true)]
public class Dest { public int Id { get; set; } public string Name { get; set; } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        generatedSources.Should().Contain(s => s.Contains("MapToSource"));
    }

    [Fact]
    public void MultipleAttributesOnDest()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Ignored { get; set; } public string Renamed { get; set; } }
public class Dest { public int Id { get; set; } [IgnoreMap] public string Ignored { get; set; } [MapFrom(""Renamed"")] public string AltName { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().NotContain(s => s.Contains("source.Ignored"));
        generatedSources.Should().Contain(s => s.Contains("source.Renamed"));
    }
}
