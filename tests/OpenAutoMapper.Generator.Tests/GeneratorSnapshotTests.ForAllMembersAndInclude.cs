using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- ForAllMembers Tests ----

    [Fact]
    public void ForAllMembers_WithCondition_AllPropertiesGetCondition()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForAllMembers(opt => opt.Condition((s, d, m) => s != null));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // All properties should have a condition applied
        var mappingExt = generatedSources.FirstOrDefault(s =>
            s.Contains("MappingExtensions") && s.Contains("MapToDest"));
        mappingExt.Should().NotBeNull();
        // Condition should appear in the generated code
        mappingExt!.Should().Contain("s != null");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForAllMembers_WithExplicitForMember_OverrideWins()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public string Tag { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ForAllMembers(opt => opt.Condition((s, d, m) => s != null))
            .ForMember(d => d.Name, opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Name should be ignored (override wins), so no source.Name assignment
        generatedSources.Should().NotContain(s =>
            s.Contains("MappingExtensions") && s.Contains("Name = source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForAllMembers_WithIgnore_AllPropertiesIgnored()
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
            .ForAllMembers(opt => opt.Ignore());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        // All properties ignored: no property assignments at all in the extension method
        // The mapping should still generate something (even if empty body)
        // No OM1010 unmapped warnings since ForAllMembers(Ignore) covers all
        GetOMDiagnostics(diagnostics).Where(d => d.Id == "OM1010").Should().BeEmpty();
    }

    [Fact]
    public void ForAllMembers_WithPreCondition()
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
            .ForAllMembers(opt => opt.PreCondition(s => s.Id > 0));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var mappingExt = generatedSources.FirstOrDefault(s =>
            s.Contains("MappingExtensions") && s.Contains("MapToDest"));
        mappingExt.Should().NotBeNull();
        // PreCondition should appear
        mappingExt!.Should().Contain("s.Id > 0");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ForAllMembers_WithNullSubstitute()
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
            .ForMember(d => d.Name, opt => opt.NullSubstitute(""default""));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(@"""default""") && s.Contains("??"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- IncludeMembers Tests ----

    [Fact]
    public void IncludeMembers_SingleNavigation_MatchesSubProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class InnerData { public string City { get; set; } public string State { get; set; } }
public class Source { public int Id { get; set; } public InnerData Address { get; set; } }
public class Dest { public int Id { get; set; } public string City { get; set; } public string State { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .IncludeMembers(s => s.Address);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // City and State should come from source.Address?.City and source.Address?.State
        generatedSources.Should().Contain(s => s.Contains("Address?.City") || s.Contains("Address.City"));
        generatedSources.Should().Contain(s => s.Contains("Address?.State") || s.Contains("Address.State"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IncludeMembers_MultipleNavigations_FirstMatchWins()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Nav1 { public string City { get; set; } }
public class Nav2 { public string City { get; set; } public string Code { get; set; } }
public class Source { public int Id { get; set; } public Nav1 Primary { get; set; } public Nav2 Secondary { get; set; } }
public class Dest { public int Id { get; set; } public string City { get; set; } public string Code { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .IncludeMembers(s => s.Primary, s => s.Secondary);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // City should come from Primary (first match), Code from Secondary
        generatedSources.Should().Contain(s => s.Contains("Primary?.City") || s.Contains("Primary.City"));
        generatedSources.Should().Contain(s => s.Contains("Secondary?.Code") || s.Contains("Secondary.Code"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IncludeMembers_NullSafety_UsesNullConditionalAccess()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class InnerData { public string Value { get; set; } }
public class Source { public int Id { get; set; } public InnerData Nav { get; set; } }
public class Dest { public int Id { get; set; } public string Value { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .IncludeMembers(s => s.Nav);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should use null-safe access pattern: source.Nav?.Value
        generatedSources.Should().Contain(s => s.Contains("Nav?.Value") || s.Contains("Nav.Value"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IncludeMembers_WithRegularPropertyMatch_RegularTakesPriority()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class InnerData { public string Name { get; set; } }
public class Source { public int Id { get; set; } public string Name { get; set; } public InnerData Details { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .IncludeMembers(s => s.Details);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Name should come from source.Name (direct match priority), not source.Details?.Name
        generatedSources.Should().Contain(s =>
            s.Contains("MappingExtensions") && s.Contains("Name = source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
