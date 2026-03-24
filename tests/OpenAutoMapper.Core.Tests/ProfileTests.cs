using System.Linq;
using FluentAssertions;
using OpenAutoMapper;
using OpenAutoMapper.Internal;
using Xunit;

namespace OpenAutoMapper.Core.Tests;

public class ProfileTests
{
    [Fact]
    public void CreateMap_RegistersTypeMapConfiguration()
    {
        var profile = new TestProfile();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs.Should().HaveCount(1);
        configs[0].SourceType.Should().Be(typeof(Source));
        configs[0].DestinationType.Should().Be(typeof(Dest));
    }

    [Fact]
    public void CreateMap_DefaultsMemberListToDestination()
    {
        var profile = new TestProfile();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs[0].MemberList.Should().Be(MemberList.Destination);
    }

    [Fact]
    public void CreateMap_WithMemberList_StoresSpecifiedMemberList()
    {
        var profile = new ProfileWithMemberList();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs[0].MemberList.Should().Be(MemberList.None);
    }

    [Fact]
    public void CreateProjection_RegistersTypeMapConfigurationAsProjection()
    {
        var profile = new ProjectionProfile();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs.Should().HaveCount(1);
        configs[0].SourceType.Should().Be(typeof(Source));
        configs[0].DestinationType.Should().Be(typeof(Dest));
        configs[0].IsProjection.Should().BeTrue();
    }

    [Fact]
    public void CreateMap_IsNotProjection()
    {
        var profile = new TestProfile();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs[0].IsProjection.Should().BeFalse();
    }

    [Fact]
    public void RecognizePrefixes_StoresPrefixes()
    {
        var profile = new PrefixProfile();

        profile.Prefixes.Should().Contain("Get");
        profile.Prefixes.Should().Contain("Is");
    }

    [Fact]
    public void RecognizePostfixes_StoresPostfixes()
    {
        var profile = new PostfixProfile();

        profile.Postfixes.Should().Contain("Dto");
        profile.Postfixes.Should().Contain("Model");
    }

    [Fact]
    public void AddGlobalIgnore_StoresIgnorePattern()
    {
        var profile = new GlobalIgnoreProfile();

        profile.GlobalIgnores.Should().Contain("_");
        profile.GlobalIgnores.Should().Contain("Internal");
    }

    [Fact]
    public void MultipleCreateMapCalls_RegistersAllMappings()
    {
        var profile = new MultiMapProfile();

        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        configs.Should().HaveCount(2);
        configs[0].SourceType.Should().Be(typeof(Source));
        configs[0].DestinationType.Should().Be(typeof(Dest));
        configs[1].SourceType.Should().Be(typeof(Dest));
        configs[1].DestinationType.Should().Be(typeof(Source));
    }

    // --- Test helper classes ---

    public class Source
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class Dest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Source, Dest>();
        }
    }

    private sealed class ProfileWithMemberList : Profile
    {
        public ProfileWithMemberList()
        {
            CreateMap<Source, Dest>(MemberList.None);
        }
    }

    private sealed class ProjectionProfile : Profile
    {
        public ProjectionProfile()
        {
            CreateProjection<Source, Dest>();
        }
    }

    private sealed class PrefixProfile : Profile
    {
        public PrefixProfile()
        {
            RecognizePrefixes("Get", "Is");
        }
    }

    private sealed class PostfixProfile : Profile
    {
        public PostfixProfile()
        {
            RecognizePostfixes("Dto", "Model");
        }
    }

    private sealed class GlobalIgnoreProfile : Profile
    {
        public GlobalIgnoreProfile()
        {
            AddGlobalIgnore("_");
            AddGlobalIgnore("Internal");
        }
    }

    private sealed class MultiMapProfile : Profile
    {
        public MultiMapProfile()
        {
            CreateMap<Source, Dest>();
            CreateMap<Dest, Source>();
        }
    }
}
