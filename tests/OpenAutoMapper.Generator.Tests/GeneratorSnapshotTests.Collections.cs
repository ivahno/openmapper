using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void ListToList_SameElementType()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Tags"));
        generatedSources.Should().Contain(s => s.Contains(".ToList()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ListToList_DifferentElementType()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class SourceItem { public int Id { get; set; } public string Name { get; set; } }
public class DestItem { public int Id { get; set; } public string Name { get; set; } }
public class Source { public int Id { get; set; } public List<SourceItem> Items { get; set; } }
public class Dest { public int Id { get; set; } public List<DestItem> Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<SourceItem, DestItem>();
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDestItem"));
        generatedSources.Should().Contain(s => s.Contains(".Select("));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ArrayToArray_SameElementType()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public int[] Scores { get; set; } }
public class Dest { public int Id { get; set; } public int[] Scores { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(".ToArray()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ListToArray()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public string[] Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(".ToArray()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ArrayToList()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string[] Tags { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(".ToList()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IEnumerableToList()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public IEnumerable<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(".ToList()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void NullCollection_DefaultsToEmpty()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should handle null with a fallback
        generatedSources.Should().Contain(s => s.Contains("is not null"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CollectionOfNestedObjects()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class OrderItem { public string Product { get; set; } public int Qty { get; set; } }
public class OrderItemDto { public string Product { get; set; } public int Qty { get; set; } }
public class Order { public int Id { get; set; } public List<OrderItem> Items { get; set; } }
public class OrderDto { public int Id { get; set; } public List<OrderItemDto> Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToOrderItemDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MixedProperties_ScalarAndCollection()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public List<int> Values { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public List<int> Values { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source.Values"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void HashSetToHashSet()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public HashSet<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public HashSet<string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("HashSet"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
