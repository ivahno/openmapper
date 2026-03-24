using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void TwoProfiles_DifferentTypePairs()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class SourceA { public int Id { get; set; } }
public class DestA { public int Id { get; set; } }
public class SourceB { public string Name { get; set; } }
public class DestB { public string Name { get; set; } }
public class ProfileA : Profile { public ProfileA() { CreateMap<SourceA, DestA>(); } }
public class ProfileB : Profile { public ProfileB() { CreateMap<SourceB, DestB>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("MapToDestA"));
        generatedSources.Should().Contain(s => s.Contains("MapToDestB"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void SingleProfile_MultipleCreateMapCalls()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class A { public int X { get; set; } }
public class ADto { public int X { get; set; } }
public class B { public int Y { get; set; } }
public class BDto { public int Y { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<A, ADto>(); CreateMap<B, BDto>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("MapToADto"));
        generatedSources.Should().Contain(s => s.Contains("MapToBDto"));
    }

    [Fact]
    public void ProfileInNestedNamespace()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp.Sub.Deep;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("TestApp.Sub.Deep"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ProfileWithNoCreateMapCalls_ProducesNoOutput()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class EmptyProfile : Profile { public EmptyProfile() { } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotContain(s => s.Contains("OpenAutoMapperImpl"));
        GetOMDiagnostics(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void TwoProfiles_SameSource_DifferentDest()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class DestA { public int Id { get; set; } }
public class DestB { public string Name { get; set; } }
public class ProfileA : Profile { public ProfileA() { CreateMap<Source, DestA>(); } }
public class ProfileB : Profile { public ProfileB() { CreateMap<Source, DestB>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("MapToDestA"));
        generatedSources.Should().Contain(s => s.Contains("MapToDestB"));
    }
}
