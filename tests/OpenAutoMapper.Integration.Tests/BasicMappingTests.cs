using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Integration.Tests;

public class BasicMappingTests
{
    [Fact]
    public void MapperConfiguration_CanBeCreated_WithInlineMapping()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_CanBeCreated_WithProfile()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TestProfile());
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_CanBeCreated_WithGenericProfile()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TestProfile>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void AssertConfigurationIsValid_PassesForMatchingProperties()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void AssertConfigurationIsValid_PassesForProfileWithMatchingProperties()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TestProfile>();
        });

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateMapper_ThrowsInvalidOperationException_WhenGeneratorNotWired()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        var act = () => config.CreateMapper();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mapper factory*source generator*");
    }

    [Fact]
    public void CreateMapper_WithServiceCtor_ThrowsInvalidOperationException_WhenGeneratorNotWired()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
        });

        var act = () => config.CreateMapper(t => throw new NotImplementedException());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mapper factory*source generator*");
    }

    [Fact]
    public void MapperConfiguration_CollectsMultipleMappings()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.CreateMap<Dest, Source>();
        });

        config.Should().NotBeNull();

        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_WithMemberListNone_SkipsValidation()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, DestWithExtra>(MemberList.None);
        });

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow("MemberList.None should skip validation");
    }

    [Fact]
    public void MapperConfiguration_MultipleProfiles()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TestProfile>();
            cfg.AddProfile<AnotherProfile>();
        });

        config.Should().NotBeNull();
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_ProfileWithMultipleMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MultiMapProfile>();
        });

        config.Should().NotBeNull();
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_AddMaps_ScansAssembly()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(BasicMappingTests).Assembly);
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_InlineCreateMap_WithMemberListSource()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>(MemberList.Source);
        });

        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_ReverseMap()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.CreateMap<Dest, Source>();
        });

        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_WithForMember_MapFrom()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, DestWithExtra>()
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Name));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithForMember_Ignore()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, DestWithExtra>()
                .ForMember(d => d.Description, opt => opt.Ignore());
        });

        config.Should().NotBeNull();
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_WithReverseMap()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ReverseMap();
        });

        config.Should().NotBeNull();
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void MapperConfiguration_WithInclude()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AnimalSource, AnimalDest>()
                .Include<DogSource, DogDest>();
            cfg.CreateMap<DogSource, DogDest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithIncludeBase()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AnimalSource, AnimalDest>();
            cfg.CreateMap<DogSource, DogDest>()
                .IncludeBase<AnimalSource, AnimalDest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithMaxDepth()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .MaxDepth(5);
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_DictionaryProperty()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DictSource, DictDest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_EnumMapping()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EnumSource, EnumDest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithProjection()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Source, Dest>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_ProjectionWithForMember()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Source, DestWithExtra>()
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Name));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_ProjectionWithIgnore()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Source, DestWithExtra>()
                .Ignore(d => d.Description);
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithCondition()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Name, opt => opt.Condition((s, d, m) => s.Name != null));
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithBeforeAfterMap()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .BeforeMap((s, d) => { })
                .AfterMap((s, d) => { });
        });

        config.Should().NotBeNull();
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

    public class DestWithExtra
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class OtherSource
    {
        public string? Value { get; set; }
    }

    public class OtherDest
    {
        public string? Value { get; set; }
    }

    public class AnimalSource
    {
        public string? Name { get; set; }
    }

    public class AnimalDest
    {
        public string? Name { get; set; }
    }

    public class DogSource : AnimalSource
    {
        public string? Breed { get; set; }
    }

    public class DogDest : AnimalDest
    {
        public string? Breed { get; set; }
    }

    public class DictSource
    {
        public int Id { get; set; }
        public System.Collections.Generic.Dictionary<string, string>? Tags { get; set; }
    }

    public class DictDest
    {
        public int Id { get; set; }
        public System.Collections.Generic.Dictionary<string, string>? Tags { get; set; }
    }

    public enum StatusA { Active, Inactive }
    public enum StatusB { Active, Inactive }

    public class EnumSource
    {
        public StatusA Status { get; set; }
    }

    public class EnumDest
    {
        public StatusB Status { get; set; }
    }

    public class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Source, Dest>();
        }
    }

    public class AnotherProfile : Profile
    {
        public AnotherProfile()
        {
            CreateMap<OtherSource, OtherDest>();
        }
    }

    public class MultiMapProfile : Profile
    {
        public MultiMapProfile()
        {
            CreateMap<Source, Dest>();
            CreateMap<OtherSource, OtherDest>();
        }
    }

    // --- Phase 6 helper classes ---

    public class AddressInner
    {
        public string? City { get; set; }
    }

    public class SourceWithCity
    {
        public int Id { get; set; }
        public string? City { get; set; }
    }

    public class DestWithAddress
    {
        public int Id { get; set; }
        public AddressInner? Address { get; set; }
    }

    public class SourceWithGetPrefix
    {
        public int Id { get; set; }
        public string? GetName { get; set; }
    }

    public class DestForPrefix
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class ForPathProfile : Profile
    {
        public ForPathProfile()
        {
            CreateMap<SourceWithCity, DestWithAddress>()
                .ForPath(d => d.Address!.City, opt => opt.MapFrom(s => s.City));
        }
    }

    public class PrefixProfile : Profile
    {
        public PrefixProfile()
        {
            RecognizePrefixes("Get");
            CreateMap<SourceWithGetPrefix, DestForPrefix>();
        }
    }

    [Fact]
    public void MapperConfiguration_WithForPath()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ForPathProfile>();
        });

        config.Should().NotBeNull();
    }

    [Fact]
    public void MapperConfiguration_WithPrefixes()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PrefixProfile>();
        });

        config.Should().NotBeNull();
    }
}
