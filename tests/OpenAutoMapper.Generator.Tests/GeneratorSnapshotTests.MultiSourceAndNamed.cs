using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- Multi-Source Tests ----

    [Fact]
    public void MultiSource_IncludeSource_GeneratesValueTupleOverload()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Primary { public int Id { get; set; } public string Name { get; set; } }
public class Additional { public string Email { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Primary, Dest>()
            .IncludeSource<Additional>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should generate a ValueTuple overload method
        generatedSources.Should().Contain(s =>
            s.Contains("MapToDest") && s.Contains("sources"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MultiSource_WithMultipleAdditionalSources()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Primary { public int Id { get; set; } }
public class Extra1 { public string Name { get; set; } }
public class Extra2 { public string Tag { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Primary, Dest>()
            .IncludeSource<Extra1>()
            .IncludeSource<Extra2>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should generate multi-source overload with both extra types
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Named Mapping Tests ----

    [Fact]
    public void NamedMapping_DefaultMapping_StillWorks()
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
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Default mapping should work as normal
        generatedSources.Should().Contain(s => s.Contains("MapToDest"));
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperImpl"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MapperImpl_NamedMappingDispatch_Method_Exists()
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
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // MapperImpl should have a named mapping dispatch method
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();
        mapperImpl!.Should().Contain("mappingName");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MapperImpl_DefaultMapping_ExcludedFromNamedDispatch()
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
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();
        // Named dispatch method should exist but indicate no named mappings configured
        mapperImpl!.Should().Contain("No named mapping");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MapperImpl_HasMapWithMappingNameOverload()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
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
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();
        // Should have the Map<TSource, TDestination>(TSource source, string mappingName) overload
        mapperImpl!.Should().Contain("Map<TSource, TDestination>(TSource source, string mappingName)");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- IMapper Interface Compliance ----

    [Fact]
    public void MapperImpl_ImplementsAllIMapperMethods()
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
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();

        // Check all IMapper methods are present
        mapperImpl!.Should().Contain("Map<TDestination>(object source)");
        mapperImpl.Should().Contain("Map<TSource, TDestination>(TSource source)");
        mapperImpl.Should().Contain("Map<TSource, TDestination>(TSource source, TDestination destination)");
        mapperImpl.Should().Contain("Map(object source, global::System.Type sourceType, global::System.Type destinationType)");
        mapperImpl.Should().Contain("Map(object source, object destination, global::System.Type sourceType, global::System.Type destinationType)");
        mapperImpl.Should().Contain("Map<TSource, TDestination>(TSource source, string mappingName)");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void FactoryInitializer_IsGenerated()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
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
        // Should generate the factory initializer
        generatedSources.Should().Contain(s =>
            s.Contains("OpenAutoMapperFactoryInit") && s.Contains("ModuleInitializer"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
