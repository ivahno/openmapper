using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // --- NullSubstitute ---

    [Fact]
    public void NullSubstitute_EmitsCoalesceOperator()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string? Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.NullSubstitute(""N/A""));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(@"""N/A"""));
        generatedSources.Should().Contain(s => s.Contains("??"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void NullSubstitute_WithNumericValue()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public decimal? Value { get; set; } }
public class Dest { public int Id { get; set; } public decimal? Value { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Value, opt => opt.NullSubstitute(0m));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("0m"));
        generatedSources.Should().Contain(s => s.Contains("??"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void NullSubstitute_WithMapFrom()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string? FullName { get; set; } }
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
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // --- ConvertUsing ---

    [Fact]
    public void ConvertUsing_ReplacesEntireMapping()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class MyConverter : ITypeConverter<Source, Dest>
{
    public Dest Convert(Source source, Dest destination, ResolutionContext context)
    {
        return new Dest { Id = source.Id * 10, Name = source.Name + ""_converted"" };
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConvertUsing(new MyConverter());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s =>
            s.Contains("ITypeConverter") && s.Contains("new MyConverter()") && s.Contains(".Convert("));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConvertUsing_GeneratesMapperDispatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }
public class MyConverter : ITypeConverter<Source, Dest>
{
    public Dest Convert(Source source, Dest destination, ResolutionContext context)
    {
        return new Dest { Id = source.Id };
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConvertUsing(new MyConverter());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should still generate OpenAutoMapperImpl
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperImpl"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // --- MapFrom<TResolver>() ---

    [Fact]
    public void MapFromResolver_EmitsNewAndResolve()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FirstName { get; set; } public string LastName { get; set; } }
public class Dest { public int Id { get; set; } public string FullName { get; set; } }
public class FullNameResolver : IValueResolver<Source, Dest, string>
{
    public string Resolve(Source source, Dest destination, string destMember, ResolutionContext context)
    {
        return source.FirstName + "" "" + source.LastName;
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.FullName, opt => opt.MapFrom<FullNameResolver>());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("FullNameResolver"));
        generatedSources.Should().Contain(s => s.Contains(".Resolve("));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MapFromResolver_WithOtherProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Computed { get; set; } }
public class ComputedResolver : IValueResolver<Source, Dest, string>
{
    public string Resolve(Source source, Dest destination, string destMember, ResolutionContext context)
    {
        return source.Id.ToString() + ""-"" + source.Name;
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Computed, opt => opt.MapFrom<ComputedResolver>());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Regular properties should still map normally
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        // Computed should use resolver
        generatedSources.Should().Contain(s => s.Contains("ComputedResolver"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // --- Integration tests ---

    [Fact]
    public void MapperConfiguration_WithNullSubstitute()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string? Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.NullSubstitute(""default""));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MapFromResolver_SuppressesUnmappedWarning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public string Extra { get; set; } }
public class ExtraResolver : IValueResolver<Source, Dest, string>
{
    public string Resolve(Source source, Dest destination, string destMember, ResolutionContext context)
    {
        return ""resolved"";
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Extra, opt => opt.MapFrom<ExtraResolver>());
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Where(d => d.Id == "OM1010").Should().BeEmpty(
            "MapFrom<TResolver>() should suppress unmapped warning for the target property");
    }
}
