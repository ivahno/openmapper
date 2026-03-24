using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void ReadOnlyDestProperty_Skipped()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string ReadOnly { get; set; } }
public class Dest { public int Id { get; set; } public string ReadOnly { get; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Where(s => s.Contains("MapToDest")).Should().NotContain(s => s.Contains("ReadOnly = "));
    }

    [Fact]
    public void StaticProperty_Skipped()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public static string StaticProp { get; set; } }
public class Dest { public int Id { get; set; } public static string StaticProp { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void RecordTypes()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public record Source(int Id, string Name) { }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
    }

    [Fact]
    public void StructSource_StructDest()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public struct SourceStruct { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<SourceStruct, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
    }

    [Fact]
    public void EmptySourceClass()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1010");
    }

    [Fact]
    public void EmptyDestClass()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotContain(s => s.Contains("MapToDest"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void PropertyNameWithUnderscore()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int _id { get; set; } }
public class Dest { public int _id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source._id"));
    }

    [Fact]
    public void SourceAndDest_SameNamespace()
    {
        var source = @"
using OpenAutoMapper;
namespace SameNs;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("namespace SameNs"));
    }

    [Fact]
    public void SourceAndDest_DifferentNamespaces()
    {
        var source = @"
using OpenAutoMapper;
namespace Source.Ns { public class Source { public int Id { get; set; } } }
namespace Dest.Ns { public class Dest { public int Id { get; set; } } }
namespace TestApp {
    public class TestProfile : Profile { public TestProfile() { CreateMap<Source.Ns.Source, Dest.Ns.Dest>(); } }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ManyProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int A { get; set; } public int B { get; set; } public int C { get; set; } public int D { get; set; } public int E { get; set; } public int F { get; set; } public int G { get; set; } public int H { get; set; } }
public class Dest { public int A { get; set; } public int B { get; set; } public int C { get; set; } public int D { get; set; } public int E { get; set; } public int F { get; set; } public int G { get; set; } public int H { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void PrivateSetterOnDest_Skipped()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string PrivSet { get; set; } }
public class Dest { public int Id { get; set; } public string PrivSet { get; private set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Where(s => s.Contains("MapToDest")).Should().NotContain(s => s.Contains("PrivSet = "));
    }

    [Fact]
    public void BooleanProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public bool IsActive { get; set; } }
public class Dest { public bool IsActive { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.IsActive"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DateTimeProperty()
    {
        var source = @"
using System;
using OpenAutoMapper;
namespace TestApp;
public class Source { public DateTime CreatedAt { get; set; } }
public class Dest { public DateTime CreatedAt { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void GuidProperty()
    {
        var source = @"
using System;
using OpenAutoMapper;
namespace TestApp;
public class Source { public Guid Key { get; set; } }
public class Dest { public Guid Key { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
