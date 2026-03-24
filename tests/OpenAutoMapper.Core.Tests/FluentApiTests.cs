using System;
using System.Linq;
using FluentAssertions;
using OpenAutoMapper;
using OpenAutoMapper.Internal;
using Xunit;

namespace OpenAutoMapper.Core.Tests;

public class FluentApiTests
{
    // --- MappingExpression fluent methods ---

    [Fact]
    public void ForMember_MapFrom_StoresSourceMemberName()
    {
        var profile = new ForMemberProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "FullName");

        pm.SourceMemberName.Should().Be("Name");
    }

    [Fact]
    public void ForMember_Ignore_SetsIsIgnored()
    {
        var profile = new ForMemberIgnoreProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Secret");

        pm.IsIgnored.Should().BeTrue();
    }

    [Fact]
    public void ForMember_Condition_StoresConditionDelegate()
    {
        var profile = new ConditionProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");

        pm.Condition.Should().NotBeNull();
    }

    [Fact]
    public void ForMember_PreCondition_StoresPreConditionDelegate()
    {
        var profile = new PreConditionProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");

        pm.PreCondition.Should().NotBeNull();
    }

    [Fact]
    public void ForMember_NullSubstitute_StoresValue()
    {
        var profile = new NullSubstituteProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");

        pm.NullSubstitute.Should().Be("N/A");
    }

    [Fact]
    public void ForMember_MapFromResolver_StoresResolverType()
    {
        var profile = new ResolverProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");

        pm.ValueResolverType.Should().Be(typeof(StubResolver));
    }

    [Fact]
    public void ForMember_UseDestinationValue_SetsFlag()
    {
        var profile = new UseDestValueProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");

        pm.UseDestinationValue.Should().BeTrue();
    }

    // --- Mapping-level fluent methods ---

    [Fact]
    public void Condition_StoresOnTypeMapConfiguration()
    {
        var profile = new MapLevelConditionProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].Condition.Should().NotBeNull();
    }

    [Fact]
    public void PreCondition_StoresOnTypeMapConfiguration()
    {
        var profile = new MapLevelPreConditionProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].PreCondition.Should().NotBeNull();
    }

    [Fact]
    public void NullSubstitute_StoresOnTypeMapConfiguration()
    {
        var profile = new MapLevelNullSubProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].NullSubstitute.Should().Be("default");
    }

    [Fact]
    public void ConstructUsing_StoresDelegate()
    {
        var profile = new ConstructUsingProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].ConstructUsing.Should().NotBeNull();
    }

    [Fact]
    public void BeforeMap_StoresDelegate()
    {
        var profile = new BeforeMapProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].BeforeMap.Should().NotBeNull();
    }

    [Fact]
    public void AfterMap_StoresDelegate()
    {
        var profile = new AfterMapProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].AfterMap.Should().NotBeNull();
    }

    [Fact]
    public void MaxDepth_StoresValue()
    {
        var profile = new MaxDepthProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].MaxDepth.Should().Be(5);
    }

    [Fact]
    public void Include_AddsToIncludedDerivedTypes()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .Include<DerivedSource, DerivedDest>();
            cfg.CreateMap<DerivedSource, DerivedDest>();
        });

        config.TypeMaps[0].IncludedDerivedTypes.Should().HaveCount(1);
        config.TypeMaps[0].IncludedDerivedTypes[0].SourceType.Should().Be(typeof(DerivedSource));
        config.TypeMaps[0].IncludedDerivedTypes[0].DestinationType.Should().Be(typeof(DerivedDest));
    }

    [Fact]
    public void IncludeBase_AddsToIncludedBaseTypes()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.CreateMap<DerivedSource, DerivedDest>()
                .IncludeBase<Source, Dest>();
        });

        // The second type map should have included base types
        config.TypeMaps.Should().HaveCount(2);
        config.TypeMaps[1].IncludedBaseTypes.Should().HaveCount(1);
        config.TypeMaps[1].IncludedBaseTypes[0].SourceType.Should().Be(typeof(Source));
        config.TypeMaps[1].IncludedBaseTypes[0].DestinationType.Should().Be(typeof(Dest));
    }

    [Fact]
    public void ReverseMap_CreatesReverseConfiguration()
    {
        var profile = new ReverseMapProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs.Should().HaveCount(2);
        configs[0].SourceType.Should().Be(typeof(Source));
        configs[0].DestinationType.Should().Be(typeof(Dest));
        configs[0].ReverseMapConfiguration.Should().NotBeNull();
        configs[1].SourceType.Should().Be(typeof(Dest));
        configs[1].DestinationType.Should().Be(typeof(Source));
    }

    [Fact]
    public void ConvertUsing_StoresConverter()
    {
        var profile = new ConvertUsingProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].ConvertUsing.Should().NotBeNull();
    }

    [Fact]
    public void ForPath_CreatesPathMap()
    {
        var profile = new ForPathProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].PathMaps.Should().HaveCount(1);
        configs[0].PathMaps[0].DestinationPath.Should().Be("Address.City");
    }

    [Fact]
    public void MapFrom_Shorthand_StoresMapping()
    {
        var profile = new MapFromShorthandProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "FullName");

        pm.SourceMemberName.Should().Be("Name");
    }

    [Fact]
    public void Ignore_Shorthand_SetsIgnored()
    {
        var profile = new IgnoreShorthandProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Secret");

        pm.IsIgnored.Should().BeTrue();
    }

    // --- ProjectionExpression ---

    [Fact]
    public void Projection_ForMember_StoresPropertyMap()
    {
        var profile = new ProjectionForMemberProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "FullName");

        pm.SourceMemberName.Should().Be("Name");
    }

    [Fact]
    public void Projection_Ignore_SetsIgnored()
    {
        var profile = new ProjectionIgnoreProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Secret");

        pm.IsIgnored.Should().BeTrue();
    }

    [Fact]
    public void Projection_ForPath_CreatesPathMap()
    {
        var profile = new ProjectionForPathProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].PathMaps.Should().HaveCount(1);
        configs[0].PathMaps[0].DestinationPath.Should().Be("Address.City");
    }

    [Fact]
    public void Projection_MapFrom_StoresMapping()
    {
        var profile = new ProjectionMapFromProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "FullName");

        pm.SourceMemberName.Should().Be("Name");
    }

    // --- MapperConfigurationExpression ---

    [Fact]
    public void AddMaps_ScansAssemblyForProfiles()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(FluentApiTests).Assembly);
        });

        config.Profiles.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateProjection_Inline_CreatesProjectionConfig()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Source, Dest>();
        });

        config.TypeMaps.Should().HaveCount(1);
        config.TypeMaps[0].IsProjection.Should().BeTrue();
    }

    [Fact]
    public void AssertConfigurationIsValid_WithProfileName_DoesNotThrowForValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SimpleProfile>();
        });

        var act = () => config.AssertConfigurationIsValid(nameof(SimpleProfile));
        act.Should().NotThrow();
    }

    [Fact]
    public void ForMember_SamePropertyTwice_ReusesPropertyMap()
    {
        var profile = new DuplicateForMemberProfile();
        var configs = profile.TypeMapConfigurationsUntyped.Cast<TypeMapConfiguration>().ToList();

        configs[0].PropertyMaps.Where(p => p.DestinationMemberName == "Name").Should().HaveCount(1);
        var pm = configs[0].PropertyMaps.First(p => p.DestinationMemberName == "Name");
        pm.NullSubstitute.Should().Be("fallback");
    }

    // --- Helper types ---

    public class Source { public int Id { get; set; } public string? Name { get; set; } public string? Secret { get; set; } }
    public class Dest { public int Id { get; set; } public string? Name { get; set; } public string? FullName { get; set; } public string? Secret { get; set; } }
    public class DerivedSource : Source { public string? Extra { get; set; } }
    public class DerivedDest : Dest { public string? Extra { get; set; } }
    public class Address { public string? City { get; set; } }
    public class DestWithAddress { public int Id { get; set; } public Address? Address { get; set; } }

    public class StubResolver : IValueResolver<Source, Dest, string?>
    {
        public string? Resolve(Source source, Dest destination, string? destMember, ResolutionContext context) => "";
    }

    public class StubConverter : ITypeConverter<Source, Dest>
    {
        public Dest Convert(Source source, Dest destination, ResolutionContext context) => new();
    }

    public sealed class SimpleProfile : Profile { public SimpleProfile() { CreateMap<Source, Dest>(); } }
    sealed class ForMemberProfile : Profile { public ForMemberProfile() { CreateMap<Source, Dest>().ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name)); } }
    sealed class ForMemberIgnoreProfile : Profile { public ForMemberIgnoreProfile() { CreateMap<Source, Dest>().ForMember(d => d.Secret, opt => opt.Ignore()); } }
    sealed class ConditionProfile : Profile { public ConditionProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.Condition((s, d, m) => s.Name != null)); } }
    sealed class PreConditionProfile : Profile { public PreConditionProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.PreCondition(s => s.Name != null)); } }
    sealed class NullSubstituteProfile : Profile { public NullSubstituteProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.NullSubstitute("N/A")); } }
    sealed class ResolverProfile : Profile { public ResolverProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.MapFrom<StubResolver>()); } }
    sealed class UseDestValueProfile : Profile { public UseDestValueProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.UseDestinationValue()); } }
    sealed class MapLevelConditionProfile : Profile { public MapLevelConditionProfile() { CreateMap<Source, Dest>().Condition((s, d) => s.Id > 0); } }
    sealed class MapLevelPreConditionProfile : Profile { public MapLevelPreConditionProfile() { CreateMap<Source, Dest>().PreCondition(s => s.Id > 0); } }
    sealed class MapLevelNullSubProfile : Profile { public MapLevelNullSubProfile() { CreateMap<Source, Dest>().NullSubstitute("default"); } }
    sealed class ConstructUsingProfile : Profile { public ConstructUsingProfile() { CreateMap<Source, Dest>().ConstructUsing(s => new Dest()); } }
    sealed class BeforeMapProfile : Profile { public BeforeMapProfile() { CreateMap<Source, Dest>().BeforeMap((s, d) => { }); } }
    sealed class AfterMapProfile : Profile { public AfterMapProfile() { CreateMap<Source, Dest>().AfterMap((s, d) => { }); } }
    sealed class MaxDepthProfile : Profile { public MaxDepthProfile() { CreateMap<Source, Dest>().MaxDepth(5); } }
    sealed class ReverseMapProfile : Profile { public ReverseMapProfile() { CreateMap<Source, Dest>().ReverseMap(); } }
    sealed class ConvertUsingProfile : Profile { public ConvertUsingProfile() { CreateMap<Source, Dest>().ConvertUsing(new StubConverter()); } }
    sealed class ForPathProfile : Profile { public ForPathProfile() { CreateMap<Source, DestWithAddress>().ForPath(d => d.Address!.City, opt => opt.MapFrom(s => s.Name)); } }
    sealed class MapFromShorthandProfile : Profile { public MapFromShorthandProfile() { CreateMap<Source, Dest>().MapFrom(d => d.FullName, s => s.Name); } }
    sealed class IgnoreShorthandProfile : Profile { public IgnoreShorthandProfile() { CreateMap<Source, Dest>().Ignore(d => d.Secret); } }
    sealed class ProjectionForMemberProfile : Profile { public ProjectionForMemberProfile() { CreateProjection<Source, Dest>().ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name)); } }
    sealed class ProjectionIgnoreProfile : Profile { public ProjectionIgnoreProfile() { CreateProjection<Source, Dest>().Ignore(d => d.Secret); } }
    sealed class ProjectionForPathProfile : Profile { public ProjectionForPathProfile() { CreateProjection<Source, DestWithAddress>().ForPath(d => d.Address!.City, opt => opt.MapFrom(s => s.Name)); } }
    sealed class ProjectionMapFromProfile : Profile { public ProjectionMapFromProfile() { CreateProjection<Source, Dest>().MapFrom(d => d.FullName, s => s.Name); } }
    sealed class DuplicateForMemberProfile : Profile { public DuplicateForMemberProfile() { CreateMap<Source, Dest>().ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name)).ForMember(d => d.Name, opt => opt.NullSubstitute("fallback")); } }
}
