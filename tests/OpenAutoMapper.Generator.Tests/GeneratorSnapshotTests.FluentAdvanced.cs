using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void Condition_MemberLevel_WrapsAssignment()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public bool IsActive { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.Condition((s, d, m) => s.IsActive));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should generate condition check in the two-pass path
        generatedSources.Should().Contain(s => s.Contains("s.IsActive"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Condition_SourceOnly_PreCondition()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public bool IsValid { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Name, opt => opt.PreCondition(s => s.IsValid));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("s.IsValid"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void BeforeMap_EmitsBeforeMappingCode()
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
        CreateMap<Source, Dest>()
            .BeforeMap((s, d) => d.Name = ""before"");
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should use two-pass mode with BeforeMap
        generatedSources.Should().Contain(s => s.Contains("var result = new"));
        generatedSources.Should().Contain(s => s.Contains(@"d.Name = ""before"""));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AfterMap_EmitsAfterMappingCode()
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
        CreateMap<Source, Dest>()
            .AfterMap((s, d) => d.Name = s.Name + ""_mapped"");
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("var result = new"));
        generatedSources.Should().Contain(s => s.Contains(@"s.Name + ""_mapped"""));
        generatedSources.Should().Contain(s => s.Contains("return result;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void BeforeMap_AndAfterMap_Together()
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
        CreateMap<Source, Dest>()
            .BeforeMap((s, d) => System.Console.Write(""""))
            .AfterMap((s, d) => System.Console.Write(""""));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("var result = new"));
        generatedSources.Should().Contain(s => s.Contains("return result;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConstructUsing_CustomConstructor()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest
{
    public Dest(int id) { Id = id; }
    public int Id { get; set; }
    public string Name { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConstructUsing(s => new Dest(s.Id));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should use the ConstructUsing lambda
        generatedSources.Should().Contain(s => s.Contains("new Dest(s.Id)"));
        generatedSources.Should().Contain(s => s.Contains("var result ="));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConstructUsing_WithPropertyAssignments()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Extra { get; set; } }
public class Dest
{
    public Dest(int id) { Id = id; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Extra { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConstructUsing(s => new Dest(s.Id));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("new Dest(s.Id)"));
        // Properties should be assigned via statements, not initializer
        generatedSources.Should().Contain(s => s.Contains("result.Name = source.Name;"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Condition_WithMapFrom()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } public bool IsActive { get; set; } }
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

    [Fact]
    public void Condition_WithIgnore_NoConflict()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Secret { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Secret { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForMember(d => d.Secret, opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().NotContain(s =>
            s.Contains("MappingExtensions") && s.Contains("Secret ="));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConstructUsing_WithForMember()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string FullName { get; set; } }
public class Dest
{
    public Dest(int id) { Id = id; }
    public int Id { get; set; }
    public string Name { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConstructUsing(s => new Dest(s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("new Dest(s.Id)"));
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AfterMap_InMapperImplementation()
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
        CreateMap<Source, Dest>()
            .AfterMap((s, d) => d.Name = d.Name + ""!"");
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Both the extension method and the mapper impl should handle AfterMap
        generatedSources.Should().Contain(s =>
            s.Contains("MappingExtensions") && s.Contains(@"d.Name + ""!"""));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConstructUsing_GeneratedCode_Compiles()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest
{
    public Dest() { }
    public int Id { get; set; }
    public string Name { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConstructUsing(s => new Dest());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("new Dest()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
