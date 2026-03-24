using FluentAssertions;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void DirectConversion_SameType()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Value { get; set; } }
public class Dest { public int Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("source.Value"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ImplicitCast_IntToLong()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Value { get; set; } }
public class Dest { public long Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Value"));
    }

    [Fact]
    public void ExplicitCast_LongToInt()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public long Value { get; set; } }
public class Dest { public int Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("(int)source.Value"));
    }

    [Fact]
    public void ToString_IntToString()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Value { get; set; } }
public class Dest { public string Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("ToString()"));
    }

    [Fact]
    public void EnumParse_StringToEnum()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Status { Active, Inactive }
public class Source { public string Status { get; set; } }
public class Dest { public Status Status { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Enum.Parse"));
    }

    [Fact]
    public void EnumByName_EnumToEnum()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum StatusA { Active, Inactive }
public enum StatusB { Active, Inactive }
public class Source { public StatusA Status { get; set; } }
public class Dest { public StatusB Status { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Phase 3: enum-to-enum now emits a compile-time switch instead of Enum.Parse
        generatedSources.Should().Contain(s => s.Contains("switch"));
        generatedSources.Should().Contain(s => s.Contains("StatusA.Active => TestApp.StatusB.Active"));
    }

    [Fact]
    public void NestedObjectMapping()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Inner { public int Id { get; set; } }
public class InnerDto { public int Id { get; set; } }
public class Source { public Inner Child { get; set; } }
public class Dest { public InnerDto Child { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); CreateMap<Inner, InnerDto>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToInnerDto"));
    }

    [Fact]
    public void NullableUnwrap_NullableIntToInt()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int? Value { get; set; } }
public class Dest { public int Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("GetValueOrDefault"));
    }

    [Fact]
    public void NullableWrap_IntToNullableInt()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Value { get; set; } }
public class Dest { public int? Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Value"));
    }

    [Fact]
    public void NullableConvert_NullableIntToNullableLong()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int? Value { get; set; } }
public class Dest { public long? Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("HasValue"));
    }

    [Fact]
    public void NullableToString()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int? Value { get; set; } }
public class Dest { public string Value { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("ToString"));
    }

    [Fact]
    public void EnumToString()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Status { Active, Inactive }
public class Source { public Status Status { get; set; } }
public class Dest { public string Status { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("ToString()"));
    }

    [Fact]
    public void DecimalToString()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public decimal Amount { get; set; } }
public class Dest { public string Amount { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("ToString()"));
    }
}
