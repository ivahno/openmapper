using System;
using System.Reflection;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class AttributeTests
{
    // --- AutoMapAttribute ---

    [Fact]
    public void AutoMapAttribute_Constructor_SetsSourceType()
    {
        var attr = new AutoMapAttribute(typeof(string));

        attr.SourceType.Should().Be(typeof(string));
    }

    [Fact]
    public void AutoMapAttribute_Constructor_ThrowsOnNullSourceType()
    {
        var act = () => new AutoMapAttribute(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("sourceType");
    }

    [Fact]
    public void AutoMapAttribute_MemberList_DefaultsToDestination()
    {
        var attr = new AutoMapAttribute(typeof(string));

        attr.MemberList.Should().Be(MemberList.Destination);
    }

    [Fact]
    public void AutoMapAttribute_MemberList_CanBeSet()
    {
        var attr = new AutoMapAttribute(typeof(string)) { MemberList = MemberList.None };

        attr.MemberList.Should().Be(MemberList.None);
    }

    [Fact]
    public void AutoMapAttribute_ReverseMap_DefaultsToFalse()
    {
        var attr = new AutoMapAttribute(typeof(string));

        attr.ReverseMap.Should().BeFalse();
    }

    [Fact]
    public void AutoMapAttribute_ReverseMap_CanBeSet()
    {
        var attr = new AutoMapAttribute(typeof(string)) { ReverseMap = true };

        attr.ReverseMap.Should().BeTrue();
    }

    [Fact]
    public void AutoMapAttribute_HasAttributeUsage_ClassOnly()
    {
        var usage = typeof(AutoMapAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Class);
    }

    // --- IgnoreMapAttribute ---

    [Fact]
    public void IgnoreMapAttribute_CanBeConstructed()
    {
        var attr = new IgnoreMapAttribute();

        attr.Should().NotBeNull();
    }

    [Fact]
    public void IgnoreMapAttribute_HasAttributeUsage_PropertyAndField()
    {
        var usage = typeof(IgnoreMapAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Property | AttributeTargets.Field);
    }

    // --- IgnoreAttribute ---

    [Fact]
    public void IgnoreAttribute_DerivesFromIgnoreMapAttribute()
    {
        var attr = new IgnoreAttribute();

        attr.Should().BeAssignableTo<IgnoreMapAttribute>();
    }

    [Fact]
    public void IgnoreAttribute_HasAttributeUsage_PropertyAndField()
    {
        var usage = typeof(IgnoreAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Property | AttributeTargets.Field);
    }

    // --- MapFromAttribute ---

    [Fact]
    public void MapFromAttribute_Constructor_SetsSourceMemberName()
    {
        var attr = new MapFromAttribute("FirstName");

        attr.SourceMemberName.Should().Be("FirstName");
    }

    [Fact]
    public void MapFromAttribute_Constructor_ThrowsOnNullSourceMemberName()
    {
        var act = () => new MapFromAttribute(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("sourceMemberName");
    }

    [Fact]
    public void MapFromAttribute_HasAttributeUsage_PropertyOnly()
    {
        var usage = typeof(MapFromAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Property);
    }

    // --- SensitivePropertyAttribute ---

    [Fact]
    public void SensitivePropertyAttribute_CanBeConstructed()
    {
        var attr = new SensitivePropertyAttribute();

        attr.Should().NotBeNull();
    }

    [Fact]
    public void SensitivePropertyAttribute_HasAttributeUsage_PropertyOnly()
    {
        var usage = typeof(SensitivePropertyAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Property);
    }

    // --- ValueResolverAttribute ---

    [Fact]
    public void ValueResolverAttribute_Constructor_SetsResolverType()
    {
        var attr = new ValueResolverAttribute(typeof(int));

        attr.ResolverType.Should().Be(typeof(int));
    }

    [Fact]
    public void ValueResolverAttribute_Constructor_ThrowsOnNullResolverType()
    {
        var act = () => new ValueResolverAttribute(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("resolverType");
    }

    [Fact]
    public void ValueResolverAttribute_HasAttributeUsage_PropertyOnly()
    {
        var usage = typeof(ValueResolverAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Property);
    }
}
