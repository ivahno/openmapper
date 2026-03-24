using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

/// <summary>
/// Tests Profile.CreateMap/CreateProjection with the Core factory wired up.
/// Exercises the Abstractions code paths that require Core to be loaded.
/// </summary>
public class ProfileCreateMapTests
{
    [Fact]
    public void CreateMap_ViaProfile_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SimpleProfile>());
        config.Should().NotBeNull();
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateMap_WithMemberListNone_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MemberListNoneProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateMap_WithMemberListSource_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MemberListSourceProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateProjection_ViaProfile_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProjectionProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateProjection_WithMemberListNone_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProjectionMemberListProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithForMember_MapFrom_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ForMemberProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithForMember_Ignore_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<IgnoreProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateMap_WithReverseMap_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ReverseMapProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateMap_WithMaxDepth_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MaxDepthProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithBeforeAfterMap_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BeforeAfterProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithConstructUsing_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ConstructUsingProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithConvertUsing_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ConvertUsingProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithCondition_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ConditionProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithPreCondition_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PreConditionProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithNullSubstitute_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<NullSubProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithForPath_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ForPathProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithInclude_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<IncludeProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateMap_WithShorthandIgnoreAndMapFrom_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ShorthandProfile>());
        config.Should().NotBeNull();
    }

    [Fact]
    public void RecognizePrefixes_InProfile_Succeeds()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PrefixPostfixProfile>());
        config.Should().NotBeNull();
    }

    // --- Helper types ---
    public class Src { public int Id { get; set; } public string? Name { get; set; } }
    public class Dst { public int Id { get; set; } public string? Name { get; set; } public string? FullName { get; set; } }
    public class DerivedSrc : Src { public string? Extra { get; set; } }
    public class DerivedDst : Dst { public string? Extra { get; set; } }
    public class Inner { public string? City { get; set; } }
    public class DstWithInner { public int Id { get; set; } public Inner? Address { get; set; } }

    sealed class SimpleProfile : Profile { public SimpleProfile() { CreateMap<Src, Dst>(); } }
    sealed class MemberListNoneProfile : Profile { public MemberListNoneProfile() { CreateMap<Src, Dst>(MemberList.None); } }
    sealed class MemberListSourceProfile : Profile { public MemberListSourceProfile() { CreateMap<Src, Dst>(MemberList.Source); } }
    sealed class ProjectionProfile : Profile { public ProjectionProfile() { CreateProjection<Src, Dst>(); } }
    sealed class ProjectionMemberListProfile : Profile { public ProjectionMemberListProfile() { CreateProjection<Src, Dst>(MemberList.None); } }
    sealed class ForMemberProfile : Profile { public ForMemberProfile() { CreateMap<Src, Dst>().ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name)); } }
    sealed class IgnoreProfile : Profile { public IgnoreProfile() { CreateMap<Src, Dst>().ForMember(d => d.FullName, opt => opt.Ignore()); } }
    sealed class ReverseMapProfile : Profile { public ReverseMapProfile() { CreateMap<Src, Dst>().ReverseMap(); } }
    sealed class MaxDepthProfile : Profile { public MaxDepthProfile() { CreateMap<Src, Dst>().MaxDepth(3); } }
    sealed class BeforeAfterProfile : Profile { public BeforeAfterProfile() { CreateMap<Src, Dst>().BeforeMap((s, d) => { }).AfterMap((s, d) => { }); } }
    sealed class ConstructUsingProfile : Profile { public ConstructUsingProfile() { CreateMap<Src, Dst>().ConstructUsing(s => new Dst()); } }
    sealed class ConvertUsingProfile : Profile { public ConvertUsingProfile() { CreateMap<Src, Dst>().ConvertUsing(new StubConv()); } }
    sealed class ConditionProfile : Profile { public ConditionProfile() { CreateMap<Src, Dst>().Condition((s, d) => s.Id > 0); } }
    sealed class PreConditionProfile : Profile { public PreConditionProfile() { CreateMap<Src, Dst>().PreCondition(s => s.Id > 0); } }
    sealed class NullSubProfile : Profile { public NullSubProfile() { CreateMap<Src, Dst>().NullSubstitute(new Dst()); } }
    sealed class ForPathProfile : Profile { public ForPathProfile() { CreateMap<Src, DstWithInner>().ForPath(d => d.Address!.City, opt => opt.MapFrom(s => s.Name)); } }
    sealed class IncludeProfile : Profile { public IncludeProfile() { CreateMap<Src, Dst>().Include<DerivedSrc, DerivedDst>(); CreateMap<DerivedSrc, DerivedDst>(); } }
    sealed class ShorthandProfile : Profile { public ShorthandProfile() { CreateMap<Src, Dst>().Ignore(d => d.FullName).MapFrom(d => d.Name, s => s.Name); } }
    sealed class PrefixPostfixProfile : Profile { public PrefixPostfixProfile() { RecognizePrefixes("Get"); RecognizePostfixes("Dto"); CreateMap<Src, Dst>(); } }

    sealed class StubConv : ITypeConverter<Src, Dst> { public Dst Convert(Src source, Dst destination, ResolutionContext context) => new(); }
}
