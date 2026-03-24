using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void DictToDict_SameKeyValue_ShallowCopy()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("new global::System.Collections.Generic.Dictionary<string, string>(source.Props)"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictToDict_NullSafe_DefaultsToEmpty()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public Dictionary<string, int> Data { get; set; } }
public class Dest { public Dictionary<string, int> Data { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("is not null"));
        generatedSources.Should().Contain(s => s.Contains("new global::System.Collections.Generic.Dictionary<string, int>()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictToDict_StringString()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public Dictionary<string, string> Tags { get; set; } }
public class Dest { public Dictionary<string, string> Tags { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, string>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictToDict_StringInt()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public Dictionary<string, int> Counts { get; set; } }
public class Dest { public Dictionary<string, int> Counts { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, int>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictPropertyOnClass_Mapped()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } public Dictionary<string, string> Metadata { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } public Dictionary<string, string> Metadata { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source.Metadata"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IDictionaryInterface_Detected()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public IDictionary<string, string> Props { get; set; } }
public class Dest { public IDictionary<string, string> Props { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, string>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictAndScalarMixed()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Count { get; set; } public Dictionary<string, int> Values { get; set; } }
public class Dest { public int Count { get; set; } public Dictionary<string, int> Values { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Count"));
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, int>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictToDict_GeneratedCode_Compiles()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public Dictionary<string, object> Props { get; set; } }
public class Dest { public Dictionary<string, object> Props { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id.StartsWith("CS", StringComparison.Ordinal)).Should().BeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IReadOnlyDictionary_Detected()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public IReadOnlyDictionary<string, string> Props { get; set; } }
public class Dest { public IReadOnlyDictionary<string, string> Props { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("Dictionary<string, string>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DictToDict_MapperImpl_Dispatches()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class Dest { public int Id { get; set; } public Dictionary<string, string> Props { get; set; } }
public class TestProfile : Profile { public TestProfile() { CreateMap<Source, Dest>(); } }
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();
        mapperImpl.Should().Contain("MapToDest");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
