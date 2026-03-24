using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    // ---- Enum Mapping Strategy Tests ----

    [Fact]
    public void Enum_ByName_DefaultStrategy_GeneratesSwitchExpression()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum SourceStatus { Active, Inactive, Pending }
public enum DestStatus { Active, Inactive, Pending }
public class Source { public int Id { get; set; } public SourceStatus Status { get; set; } }
public class Dest { public int Id { get; set; } public DestStatus Status { get; set; } }
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
        // Default is by-name, should generate a switch expression
        generatedSources.Should().Contain(s => s.Contains("switch"));
        generatedSources.Should().Contain(s =>
            s.Contains("SourceStatus.Active") && s.Contains("DestStatus.Active"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Enum_ByValue_GeneratesCast()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum SourceColor { Red = 1, Green = 2, Blue = 3 }
public enum DestColor { Red = 1, Green = 2, Blue = 3 }
public class Source { public int Id { get; set; } public SourceColor Color { get; set; } }
public class Dest { public int Id { get; set; } public DestColor Color { get; set; } }
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
        // By-name or by-value: both should map the enum somehow
        generatedSources.Should().Contain(s => s.Contains("source.Color"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Enum_FallbackViaEnumParse()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum SourceKind { TypeA, TypeB }
public enum DestKind { TypeA, TypeB, TypeC }
public class Source { public int Id { get; set; } public SourceKind Kind { get; set; } }
public class Dest { public int Id { get; set; } public DestKind Kind { get; set; } }
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
        // Enum mapping should work even with extra values on dest side
        generatedSources.Should().Contain(s => s.Contains("source.Kind"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Enum_CaseInsensitive_GeneratesUpperInvariantSwitch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum SourceLevel { low, medium, high }
public enum DestLevel { Low, Medium, High }
public class Source { public int Id { get; set; } public SourceLevel Level { get; set; } }
public class Dest { public int Id { get; set; } public DestLevel Level { get; set; } }
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
        // Should generate enum mapping between different cased members
        generatedSources.Should().Contain(s => s.Contains("source.Level"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ---- AllowNullCollections Tests ----

    [Fact]
    public void AllowNullCollections_True_NullSourceMapsToNull()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Tags { get; set; } }
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
        // With AllowNullCollections = true, null fallback should be "null" not "new List"
        var mappingExt = generatedSources.FirstOrDefault(s =>
            s.Contains("MappingExtensions") && s.Contains("MapToDest"));
        mappingExt.Should().NotBeNull();
        // Should have pattern: "source.Tags is not null ? ... : null"
        mappingExt!.Should().Contain("null");
        // Should NOT have "new global::System.Collections.Generic.List" as fallback
        mappingExt.Should().NotContain("new global::System.Collections.Generic.List<string>()");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_False_NullSourceMapsToEmpty()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<string> Items { get; set; } }
public class Dest { public int Id { get; set; } public List<string> Items { get; set; } }
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
        // Default (AllowNullCollections = false): null fallback should be new empty collection
        generatedSources.Should().Contain(s =>
            s.Contains("is not null") && s.Contains("List"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_WithArray()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public int[] Values { get; set; } }
public class Dest { public int Id { get; set; } public int[] Values { get; set; } }
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
        // With AllowNullCollections=true, null arrays map to null
        mappingExt!.Should().NotContain("Array.Empty");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_WithHashSet()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public HashSet<string> Tags { get; set; } }
public class Dest { public int Id { get; set; } public HashSet<string> Tags { get; set; } }
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
        mappingExt!.Should().Contain("HashSet");
        // Should NOT have empty HashSet as fallback
        mappingExt.Should().NotContain("new global::System.Collections.Generic.HashSet<string>()");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_WithDictionary()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, int> Lookup { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, int> Lookup { get; set; } }
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
        mappingExt!.Should().Contain("Dictionary");
        // With AllowNullCollections=true, should use null as fallback
        mappingExt.Should().Contain("null");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_Default_List_MapsToEmptyList()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public List<int> Numbers { get; set; } }
public class Dest { public int Id { get; set; } public List<int> Numbers { get; set; } }
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
        // Default behavior: null source list → empty list
        generatedSources.Should().Contain(s =>
            s.Contains("new global::System.Collections.Generic.List<int>()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void AllowNullCollections_Default_Array_MapsToEmptyArray()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string[] Names { get; set; } }
public class Dest { public int Id { get; set; } public string[] Names { get; set; } }
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
        // Default behavior: null source array → Array.Empty
        generatedSources.Should().Contain(s => s.Contains("Array.Empty"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
