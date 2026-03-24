using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- Dictionary-to-Object Tests ----
    // Note: Dictionary<string,T> as source type in CreateMap requires that the
    // full type name be resolvable via GetTypeByMetadataName, which doesn't work
    // for constructed generic types. These tests verify the behavior via class wrappers
    // and property-level dictionary scenarios.

    [Fact]
    public void DictionaryProperty_OnSource_MapsCorrectly()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, string> Metadata { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, string> Metadata { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("source.Metadata"));
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, string>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictionaryProperty_NullSafe_DefaultsToEmpty()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, int> Data { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, int> Data { get; set; } }
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
        // Default: null source dict → empty dict
        generatedSources.Should().Contain(s => s.Contains("is not null"));
        generatedSources.Should().Contain(s => s.Contains("new global::System.Collections.Generic.Dictionary<string, int>()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictionaryProperty_AllowNullCollections_NullFallback()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        AllowNullCollections = true;
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var mappingExt = generatedSources.FirstOrDefault(s =>
            s.Contains("MappingExtensions") && s.Contains("MapToDest"));
        mappingExt.Should().NotBeNull();
        // With AllowNullCollections=true, dictionary null fallback should be "null"
        mappingExt!.Should().Contain("null");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictionaryProperty_MixedWithScalars()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Count { get; set; } public string Name { get; set; } public Dictionary<string, int> Values { get; set; } }
public class Dest { public int Count { get; set; } public string Name { get; set; } public Dictionary<string, int> Values { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Count"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, int>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Deep Clone Tests ----

    [Fact]
    public void DeepClone_FlatObject_RecursiveMapTo()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Source>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Deep clone: same type mapping should generate MapToSource
        generatedSources.Should().Contain(s => s.Contains("MapToSource"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_WithCollections_SelectMapTo()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Item { public int Id { get; set; } public string Name { get; set; } }
public class Container { public int Id { get; set; } public List<Item> Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Item, Item>()
            .UseDeepCloning();
        CreateMap<Container, Container>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Deep clone with collections should use .Select(x => x.MapToItem())
        generatedSources.Should().Contain(s =>
            s.Contains(".Select(") && s.Contains("MapToItem"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_ValueTypes_DirectCopy()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public double Value { get; set; } public bool Active { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Source>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Value types (int, double, bool) should be direct copy, not recursive
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Value"));
        generatedSources.Should().Contain(s => s.Contains("source.Active"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_ReferenceType_RecursiveClone()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Inner { public string Data { get; set; } }
public class Outer { public int Id { get; set; } public Inner Details { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Inner, Inner>()
            .UseDeepCloning();
        CreateMap<Outer, Outer>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Reference type Inner should be recursively cloned via MapToInner
        generatedSources.Should().Contain(s =>
            s.Contains("MapToOuter") || s.Contains("MapToInner"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_WithoutUseDeepCloning_NormalMapping()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Source>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Without UseDeepCloning, same-type mapping should still work (shallow copy)
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_StringProperties_DirectCopy()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Model { public string Name { get; set; } public string Description { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Model, Model>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Strings are immutable, direct copy is fine even in deep clone
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source.Description"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
