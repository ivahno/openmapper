using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- ForPath Tests ----

    [Fact]
    public void ForPath_SimpleOneLevelPath()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public string City { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("result.Address ??= new"));
        generatedSources.Should().Contain(s => s.Contains("result.Address.City = source.City;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForPath_MultiplePathsOnSameIntermediate()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public string City { get; set; } public string State { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City))
            .ForPath(d => d.Address.State, opt => opt.MapFrom(s => s.State));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("result.Address.City = source.City;"));
        generatedSources.Should().Contain(s => s.Contains("result.Address.State = source.State;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForPath_WithIgnore()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should not emit Address.City assignment
        generatedSources.Should().NotContain(s =>
            s.Contains("MappingExtensions") && s.Contains("Address.City ="));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForPath_MixedWithRegularProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public string Name { get; set; } public string City { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Regular properties should be mapped normally
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        // ForPath should generate dotted path
        generatedSources.Should().Contain(s => s.Contains("result.Address.City = source.City;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForPath_SuppressesUnmappedWarning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public string City { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City));
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        // Should not report OM1010 for Address since ForPath covers it
        var unmapped = diagnostics.Where(d => d.Id == "OM1010" && d.GetMessage(System.Globalization.CultureInfo.InvariantCulture).Contains("Address")).ToList();
        unmapped.Should().BeEmpty();
    }

    [Fact]
    public void ForPath_InMapperImplementation()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public string City { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // MapperImpl should also handle dotted paths
        generatedSources.Should().Contain(s =>
            s.Contains("OpenAutoMapperImpl") && s.Contains("Address ??= new"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForPath_InProjection()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public string City { get; set; } public string State { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Source, Dest>()
            .ForPath(d => d.Address.City, opt => opt.MapFrom(s => s.City))
            .ForPath(d => d.Address.State, opt => opt.MapFrom(s => s.State));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Projection should use nested member initializer, not ??= new
        generatedSources.Should().Contain(s =>
            s.Contains("Address = new TestApp.Address"));
        generatedSources.Should().Contain(s => s.Contains("City = source.City"));
        generatedSources.Should().Contain(s => s.Contains("State = source.State"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- Prefix/Postfix Tests ----

    [Fact]
    public void RecognizePrefixes_GetPrefix_MatchesProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string GetName { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        RecognizePrefixes(""Get"");
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.GetName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void RecognizePostfixes_DtoSuffix_MatchesProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string NameDto { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        RecognizePostfixes(""Dto"");
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.NameDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void RecognizePrefixes_MultiplePrefixes()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string GetName { get; set; } public bool IsActive { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public bool Active { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        RecognizePrefixes(""Get"", ""Is"");
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.GetName"));
        generatedSources.Should().Contain(s => s.Contains("source.IsActive"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void RecognizePrefixes_NoMatch_ReportsUnmapped()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Foo { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        RecognizePrefixes(""Get"");
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1010");
    }

    // ---- Unflattening Tests ----

    [Fact]
    public void Unflatten_SourceAddressCity_ToDestAddressDotCity()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public string AddressCity { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("Address ??= new"));
        generatedSources.Should().Contain(s => s.Contains("Address.City = source.AddressCity;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Unflatten_MultipleNestedProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public string AddressCity { get; set; } public string AddressState { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("Address.City = source.AddressCity;"));
        generatedSources.Should().Contain(s => s.Contains("Address.State = source.AddressState;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Unflatten_NoMatchDoesNotUnflatten()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public string ZipCode { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        // Should report unmapped warning for Address since no source props match
        diagnostics.Should().Contain(d => d.Id == "OM1010");
    }

    [Fact]
    public void Unflatten_EmitsNullCoalescingNew()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Source { public int Id { get; set; } public string AddressCity { get; set; } }
public class Dest { public int Id { get; set; } public Address Address { get; set; } }
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
        // Should emit ??= new for the intermediate Address type
        generatedSources.Should().Contain(s => s.Contains("??= new TestApp.Address()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- IMemberValueResolver Tests ----

    [Fact]
    public void MemberValueResolver_EmitsResolveCall()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string FullName { get; set; } }
public class NameResolver : IMemberValueResolver<Source, Dest, string, string>
{
    public string Resolve(Source source, Dest destination, string sourceMember, string destMember, ResolutionContext context)
    {
        return sourceMember?.ToUpper();
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.FullName, opt => opt.MapFrom<NameResolver, string>(s => s.Name));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should generate a Resolve call with 5 parameters including source member access
        generatedSources.Should().Contain(s =>
            s.Contains("new TestApp.NameResolver().Resolve(source, default!, source.Name, default!, default!)"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MemberValueResolver_SuppressesUnmappedWarning()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string FullName { get; set; } }
public class NameResolver : IMemberValueResolver<Source, Dest, string, string>
{
    public string Resolve(Source source, Dest destination, string sourceMember, string destMember, ResolutionContext context)
    {
        return sourceMember?.ToUpper();
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.FullName, opt => opt.MapFrom<NameResolver, string>(s => s.Name));
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        // Should not report OM1010 for FullName since it's mapped via MemberValueResolver
        var unmapped = diagnostics.Where(d => d.Id == "OM1010" && d.GetMessage(System.Globalization.CultureInfo.InvariantCulture).Contains("FullName")).ToList();
        unmapped.Should().BeEmpty();
    }
}
