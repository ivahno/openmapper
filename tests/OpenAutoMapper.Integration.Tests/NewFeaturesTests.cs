using System;
using System.Collections.Generic;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Integration.Tests;

public class NewFeaturesTests
{
    // ---- Phase 1: ForCtorParam Configuration ----

    [Fact]
    public void ForCtorParam_Configuration_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CtorSource, CtorDest>()
                .ForCtorParam("id", opt => opt.MapFrom(s => s.Identifier));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void ForCtorParam_WithMultipleParams()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CtorSource, CtorDest>()
                .ForCtorParam("id", opt => opt.MapFrom(s => s.Identifier))
                .ForCtorParam("name", opt => opt.MapFrom(s => s.Label));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void ForCtorParam_ChainedWithForMember()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CtorSource, CtorDestWithExtra>()
                .ForCtorParam("id", opt => opt.MapFrom(s => s.Identifier))
                .ForMember(d => d.Extra, opt => opt.Ignore());
        });

        config.Should().NotBeNull();
    }

    // ---- Phase 2: ForAllMembers Configuration ----

    [Fact]
    public void ForAllMembers_Configuration_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .ForAllMembers(opt => opt.Condition((s, d, m) => s != null));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void ForAllMembers_WithIgnore()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .ForAllMembers(opt => opt.Ignore());
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void ForAllMembers_WithPreCondition()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .ForAllMembers(opt => opt.PreCondition(s => s.Id > 0));
        });

        config.Should().NotBeNull();
    }

    // ---- Phase 2: IncludeMembers Configuration ----

    [Fact]
    public void IncludeMembers_Configuration_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithNav, FlatDest>()
                .IncludeMembers(s => s.Details!);
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void IncludeMembers_MultipleNavigations()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithMultiNav, FlatDest2>()
                .IncludeMembers(s => s.Primary!, s => s.Secondary!);
        });

        config.Should().NotBeNull();
    }

    // ---- Phase 4: UseDeepCloning Configuration ----

    [Fact]
    public void UseDeepCloning_Configuration_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleSource>()
                .UseDeepCloning();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void UseDeepCloning_ChainedWithOtherOptions()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleSource>()
                .UseDeepCloning()
                .MaxDepth(5);
        });

        config.Should().NotBeNull();
    }

    // ---- Phase 5: IncludeSource Configuration ----

    [Fact]
    public void IncludeSource_Configuration_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .IncludeSource<AdditionalSource>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void IncludeSource_MultipleAdditionalSources()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .IncludeSource<AdditionalSource>()
                .IncludeSource<AnotherAdditional>();
        });

        config.Should().NotBeNull();
    }

    // ---- Phase 3: AllowNullCollections ----

    [Fact]
    public void AllowNullCollections_ProfileProperty()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AllowNullCollectionsProfile>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void AllowNullCollections_DefaultProfile()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DefaultCollectionsProfile>();
        });

        config.Should().NotBeNull();
    }

    // ---- Combined features ----

    [Fact]
    public void AllFeatures_CanBeCombined()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDest>()
                .ForAllMembers(opt => opt.Condition((s, d, m) => s != null));

            cfg.CreateMap<SourceWithNav, FlatDest>()
                .IncludeMembers(s => s.Details!);

            cfg.CreateMap<SimpleSource, SimpleSource>()
                .UseDeepCloning();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithForCtorParam_AssertConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CtorSource, CtorDest>()
                .ForCtorParam("id", opt => opt.MapFrom(s => s.Identifier));
        });

        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    // ---- Test helper classes ----

    public class SimpleSource
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class SimpleDest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class CtorSource
    {
        public int Identifier { get; set; }
        public string? Label { get; set; }
    }

    public class CtorDest
    {
        public CtorDest(int id, string? name = null)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string? Name { get; }
    }

    public class CtorDestWithExtra
    {
        public CtorDestWithExtra(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public string? Extra { get; set; }
    }

    public class NavDetails
    {
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class SourceWithNav
    {
        public int Id { get; set; }
        public NavDetails? Details { get; set; }
    }

    public class FlatDest
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class NavPrimary
    {
        public string? City { get; set; }
    }

    public class NavSecondary
    {
        public string? Code { get; set; }
    }

    public class SourceWithMultiNav
    {
        public int Id { get; set; }
        public NavPrimary? Primary { get; set; }
        public NavSecondary? Secondary { get; set; }
    }

    public class FlatDest2
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? Code { get; set; }
    }

    public class AdditionalSource
    {
        public string? Email { get; set; }
    }

    public class AnotherAdditional
    {
        public string? Phone { get; set; }
    }

    public class CollectionSource
    {
        public int Id { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class CollectionDest
    {
        public int Id { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class AllowNullCollectionsProfile : Profile
    {
        public AllowNullCollectionsProfile()
        {
            AllowNullCollections = true;
            CreateMap<CollectionSource, CollectionDest>();
        }
    }

    public class DefaultCollectionsProfile : Profile
    {
        public DefaultCollectionsProfile()
        {
            CreateMap<CollectionSource, CollectionDest>();
        }
    }
}
