using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void SingleTypePair_MapperImplGenerated()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperImpl"));
        generatedSources.Should().Contain(s => s.Contains("IMapper"));
    }

    [Fact]
    public void MultipleTypePairs_AllDispatched()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class SourceA { public int Id { get; set; } }
public class DestA { public int Id { get; set; } }
public class SourceB { public string Name { get; set; } }
public class DestB { public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<SourceA, DestA>(); CreateMap<SourceB, DestB>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("MapToDestA"));
        generatedSources.Should().Contain(s => s.Contains("MapToDestB"));
    }

    [Fact]
    public void MapperImpl_HasMapSingleGeneric()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (_, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("Map<TDestination>(object source)"));
    }

    [Fact]
    public void MapperImpl_HasMapTwoGeneric()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (_, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("Map<TSource, TDestination>(TSource source)"));
    }

    [Fact]
    public void MapperImpl_HasMapTwoGenericUpdate()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (_, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s =>
            s.Contains("Map<TSource, TDestination>(TSource source, TDestination destination)"));
    }

    [Fact]
    public void MapperImpl_FactoryInitializerGenerated()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (_, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("ModuleInitializer"));
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperFactoryInit"));
    }

    [Fact]
    public void MapperImpl_NonGenericMapOverloads()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (_, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s =>
            s.Contains("Map(object source, global::System.Type sourceType, global::System.Type destinationType)"));
    }
}
