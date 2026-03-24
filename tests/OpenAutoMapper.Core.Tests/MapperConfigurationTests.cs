using System;
using FluentAssertions;
using OpenAutoMapper;
using OpenAutoMapper.Internal;
using Xunit;

namespace OpenAutoMapper.Core.Tests;

public class MapperConfigurationTests
{
    [Fact]
    public void Constructor_InvokesConfigurationAction()
    {
        bool wasInvoked = false;

        var config = new MapperConfiguration(cfg =>
        {
            wasInvoked = true;
        });

        wasInvoked.Should().BeTrue();
    }

    [Fact]
    public void Constructor_CollectsInlineTypeMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceA, DestA>();
        });

        config.TypeMaps.Should().HaveCount(1);
        config.TypeMaps[0].SourceType.Should().Be(typeof(SourceA));
        config.TypeMaps[0].DestinationType.Should().Be(typeof(DestA));
    }

    [Fact]
    public void AddProfile_RegistersProfileAndCollectsTypeMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TestMappingProfile());
        });

        config.Profiles.Should().HaveCount(1);
        config.TypeMaps.Should().HaveCount(1);
        config.TypeMaps[0].SourceType.Should().Be(typeof(SourceA));
        config.TypeMaps[0].DestinationType.Should().Be(typeof(DestA));
    }

    [Fact]
    public void AddProfile_Generic_RegistersProfileAndCollectsTypeMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TestMappingProfile>();
        });

        config.Profiles.Should().HaveCount(1);
        config.TypeMaps.Should().HaveCount(1);
    }

    [Fact]
    public void CreateMapper_ThrowsWhenMapperFactoryIsNull()
    {
        // Ensure MapperFactory is null
        MapperConfiguration.MapperFactory = null;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceA, DestA>();
        });

        var act = () => config.CreateMapper();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mapper factory*");
    }

    [Fact]
    public void CreateMapper_WithServiceCtor_ThrowsWhenMapperFactoryWithServiceCtorIsNull()
    {
        MapperConfiguration.MapperFactoryWithServiceCtor = null;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceA, DestA>();
        });

        var act = () => config.CreateMapper(t => throw new NotImplementedException());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mapper factory*");
    }

    [Fact]
    public void AssertConfigurationIsValid_DoesNotThrowForValidConfig()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceA, DestA>();
        });

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void AssertConfigurationIsValid_DoesNotThrowForMemberListNone()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceA, DestWithExtra>(MemberList.None);
        });

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    // --- Test helper classes ---

    public class SourceA
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class DestA
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class DestWithExtra
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ExtraProperty { get; set; }
    }

    public class TestMappingProfile : Profile
    {
        public TestMappingProfile()
        {
            CreateMap<SourceA, DestA>();
        }
    }
}
