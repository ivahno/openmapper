using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void EnumByName_GeneratesSwitch_NotEnumParse()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Color { Red, Green, Blue }
public enum ColorDto { Red, Green, Blue }
public class Source { public Color Color { get; set; } }
public class Dest { public ColorDto Color { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("switch"));
        generatedSources.Should().NotContain(s => s.Contains("Enum.Parse") && s.Contains("ColorDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByName_AllMembersPresent()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Status { Active, Inactive, Pending }
public enum StatusDto { Active, Inactive, Pending }
public class Source { public Status Status { get; set; } }
public class Dest { public StatusDto Status { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Status.Active"));
        generatedSources.Should().Contain(s => s.Contains("Status.Inactive"));
        generatedSources.Should().Contain(s => s.Contains("Status.Pending"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByName_DefaultArm_Throws()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Color { Red, Blue }
public enum ColorDto { Red, Blue }
public class Source { public Color Color { get; set; } }
public class Dest { public ColorDto Color { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("_ => throw"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByName_SubsetMatch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum SourceEnum { A, B, C }
public enum DestEnum { A, B }
public class Source { public SourceEnum Val { get; set; } }
public class Dest { public DestEnum Val { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should generate switch with matching members A and B
        generatedSources.Should().Contain(s => s.Contains("SourceEnum.A => TestApp.DestEnum.A"));
        generatedSources.Should().Contain(s => s.Contains("SourceEnum.B => TestApp.DestEnum.B"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumFlags_BothFlags_CastByValue()
    {
        var source = @"
using System;
using OpenAutoMapper;
namespace TestApp;
[Flags]
public enum Permissions { None = 0, Read = 1, Write = 2, Execute = 4 }
[Flags]
public enum PermissionsDto { None = 0, Read = 1, Write = 2, Execute = 4 }
public class Source { public Permissions Perms { get; set; } }
public class Dest { public PermissionsDto Perms { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Flags enums should use direct cast, not switch
        generatedSources.Should().Contain(s => s.Contains("(TestApp.PermissionsDto)(int)source.Perms"));
        generatedSources.Should().NotContain(s => s.Contains("switch") && s.Contains("Permissions"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumFlags_OneNotFlags_FallsToByName()
    {
        var source = @"
using System;
using OpenAutoMapper;
namespace TestApp;
[Flags]
public enum FlagsEnum { None = 0, A = 1, B = 2 }
public enum NonFlagsEnum { None, A, B }
public class Source { public FlagsEnum Val { get; set; } }
public class Dest { public NonFlagsEnum Val { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Only one has [Flags], so should use name-based switch
        generatedSources.Should().Contain(s => s.Contains("switch"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByName_SingleMember()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Single { OnlyOne }
public enum SingleDto { OnlyOne }
public class Source { public Single Val { get; set; } }
public class Dest { public SingleDto Val { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Single.OnlyOne => TestApp.SingleDto.OnlyOne"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByName_LargeEnum()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum LargeA { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10 }
public enum LargeB { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10 }
public class Source { public LargeA Val { get; set; } }
public class Dest { public LargeB Val { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("LargeA.V1 => TestApp.LargeB.V1"));
        generatedSources.Should().Contain(s => s.Contains("LargeA.V10 => TestApp.LargeB.V10"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumByValue_Unchanged_Regression()
    {
        // EnumByValue (same enum type) should still work the same
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum Status { Active, Inactive }
public class Source { public int StatusCode { get; set; } }
public class Dest { public int StatusCode { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.StatusCode"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumToString_Unchanged_Regression()
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
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void StringToEnum_Unchanged_Regression()
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
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void EnumFlags_GeneratedCode_Compiles()
    {
        var source = @"
using System;
using OpenAutoMapper;
namespace TestApp;
[Flags]
public enum Perms { None = 0, Read = 1, Write = 2 }
[Flags]
public enum PermsDto { None = 0, Read = 1, Write = 2 }
public class Source { public Perms Access { get; set; } }
public class Dest { public PermsDto Access { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id.StartsWith("CS", StringComparison.Ordinal)).Should().BeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
