using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void SimpleFlatDto_ExactNameMatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CaseInsensitiveMatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int id { get; set; } public string name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.id"));
        generatedSources.Should().Contain(s => s.Contains("source.name"));
    }

    [Fact]
    public void Flattening_AddressCity()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public Address Address { get; set; } }
public class Dest { public int Id { get; set; } public string AddressCity { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Address.City"));
    }

    [Fact]
    public void DeepFlattening_TwoLevels()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Inner { public string Name { get; set; } }
public class Middle { public Inner Inner { get; set; } }
public class Source { public Middle Middle { get; set; } }
public class Dest { public string MiddleInnerName { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MixedMatch_SomeUnmapped()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Extra { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Should().Contain(d => d.Id == "OM1010");
    }

    [Fact]
    public void AllUnmapped_NoExtensionGenerated()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Foo { get; set; } }
public class Dest { public int Bar { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotContain(s => s.Contains("MapToDest"));
        diagnostics.Should().Contain(d => d.Id == "OM1010");
    }

    [Fact]
    public void SourceWithInheritance()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class BaseSource { public int Id { get; set; } }
public class Source : BaseSource { public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DestinationWithInheritance()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class BaseDest { public int Id { get; set; } }
public class Dest : BaseDest { public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
    }
}
