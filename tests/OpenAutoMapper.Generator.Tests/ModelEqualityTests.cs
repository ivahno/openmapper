using System.Collections.Immutable;
using FluentAssertions;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

/// <summary>
/// Tests for Equals/GetHashCode on generator model classes.
/// </summary>
public class ModelEqualityTests
{
    // ========================================================================
    // ConstructorParamDescriptor
    // ========================================================================

    [Fact]
    public void ConstructorParamDescriptor_Equal_Instances_AreEqual()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        var b = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ConstructorParamDescriptor_DifferentParamName_NotEqual()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        var b = new ConstructorParamDescriptor("name", "int", "Id", "int", ConversionKind.Direct);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ConstructorParamDescriptor_DifferentConversionKind_NotEqual()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        var b = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.ExplicitCast);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ConstructorParamDescriptor_Null_NotEqual()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ConstructorParamDescriptor_SameReference_IsEqual()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void ConstructorParamDescriptor_ObjectEquals_WithTypeMismatch()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals((object)"not a descriptor").Should().BeFalse();
    }

    [Fact]
    public void ConstructorParamDescriptor_ObjectEquals_WithMatchingDescriptor()
    {
        var a = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        var b = new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals((object)b).Should().BeTrue();
    }

    // ========================================================================
    // CtorParamConfigReference
    // ========================================================================

    [Fact]
    public void CtorParamConfigReference_Equal_Instances_AreEqual()
    {
        var a = new CtorParamConfigReference("id", "Id");
        var b = new CtorParamConfigReference("id", "Id");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void CtorParamConfigReference_NullSourceMember_AreEqual()
    {
        var a = new CtorParamConfigReference("id", null);
        var b = new CtorParamConfigReference("id", null);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void CtorParamConfigReference_DifferentParamName_NotEqual()
    {
        var a = new CtorParamConfigReference("id", "Id");
        var b = new CtorParamConfigReference("name", "Id");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void CtorParamConfigReference_DifferentSourceMember_NotEqual()
    {
        var a = new CtorParamConfigReference("id", "Id");
        var b = new CtorParamConfigReference("id", "Identifier");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void CtorParamConfigReference_NullVsNonNull_NotEqual()
    {
        var a = new CtorParamConfigReference("id", null);
        var b = new CtorParamConfigReference("id", "Id");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void CtorParamConfigReference_Null_NotEqual()
    {
        var a = new CtorParamConfigReference("id", "Id");
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void CtorParamConfigReference_SameReference_IsEqual()
    {
        var a = new CtorParamConfigReference("id", "Id");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void CtorParamConfigReference_ObjectEquals()
    {
        var a = new CtorParamConfigReference("id", "Id");
        var b = new CtorParamConfigReference("id", "Id");
        a.Equals((object)b).Should().BeTrue();
        a.Equals((object)"wrong type").Should().BeFalse();
    }

    // ========================================================================
    // EnumMemberPair
    // ========================================================================

    [Fact]
    public void EnumMemberPair_Equal_Instances_AreEqual()
    {
        var a = new EnumMemberPair("Active", "Active");
        var b = new EnumMemberPair("Active", "Active");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EnumMemberPair_DifferentSource_NotEqual()
    {
        var a = new EnumMemberPair("Active", "Active");
        var b = new EnumMemberPair("Inactive", "Active");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EnumMemberPair_DifferentDest_NotEqual()
    {
        var a = new EnumMemberPair("Active", "Active");
        var b = new EnumMemberPair("Active", "Enabled");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EnumMemberPair_Null_NotEqual()
    {
        var a = new EnumMemberPair("Active", "Active");
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EnumMemberPair_SameReference_IsEqual()
    {
        var a = new EnumMemberPair("Active", "Active");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void EnumMemberPair_ObjectEquals()
    {
        var a = new EnumMemberPair("Active", "Active");
        a.Equals((object)new EnumMemberPair("Active", "Active")).Should().BeTrue();
        a.Equals((object)"string").Should().BeFalse();
    }

    // ========================================================================
    // IncludedTypeDescriptor
    // ========================================================================

    [Fact]
    public void IncludedTypeDescriptor_Equal_Instances_AreEqual()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        var b = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void IncludedTypeDescriptor_DifferentSourceFullName_NotEqual()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        var b = new IncludedTypeDescriptor("NS.Cat", "Dog", "NS.DogDto", "DogDto");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IncludedTypeDescriptor_DifferentDestName_NotEqual()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        var b = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.CatDto", "CatDto");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IncludedTypeDescriptor_Null_NotEqual()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void IncludedTypeDescriptor_SameReference_IsEqual()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void IncludedTypeDescriptor_ObjectEquals()
    {
        var a = new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto");
        a.Equals((object)new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto")).Should().BeTrue();
        a.Equals((object)"wrong").Should().BeFalse();
    }

    // ========================================================================
    // IncludedTypeReference
    // ========================================================================

    [Fact]
    public void IncludedTypeReference_Equal_Instances_AreEqual()
    {
        var a = new IncludedTypeReference("NS.Dog", "NS.DogDto");
        var b = new IncludedTypeReference("NS.Dog", "NS.DogDto");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void IncludedTypeReference_DifferentSource_NotEqual()
    {
        var a = new IncludedTypeReference("NS.Dog", "NS.DogDto");
        var b = new IncludedTypeReference("NS.Cat", "NS.DogDto");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void IncludedTypeReference_Null_NotEqual()
    {
        var a = new IncludedTypeReference("NS.Dog", "NS.DogDto");
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void IncludedTypeReference_SameReference_IsEqual()
    {
        var a = new IncludedTypeReference("NS.Dog", "NS.DogDto");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void IncludedTypeReference_ObjectEquals()
    {
        var a = new IncludedTypeReference("NS.Dog", "NS.DogDto");
        a.Equals((object)new IncludedTypeReference("NS.Dog", "NS.DogDto")).Should().BeTrue();
        a.Equals((object)42).Should().BeFalse();
    }

    // ========================================================================
    // MemberConfigReference
    // ========================================================================

    [Fact]
    public void MemberConfigReference_Equal_Instances_AreEqual()
    {
        var a = new MemberConfigReference("Name", "FullName", false);
        var b = new MemberConfigReference("Name", "FullName", false);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void MemberConfigReference_DifferentDestMember_NotEqual()
    {
        var a = new MemberConfigReference("Name", "FullName", false);
        var b = new MemberConfigReference("Title", "FullName", false);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_DifferentIgnored_NotEqual()
    {
        var a = new MemberConfigReference("Name", null, false);
        var b = new MemberConfigReference("Name", null, true);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_WithCondition_Equal()
    {
        var a = new MemberConfigReference("Name", "FullName", false, "s.IsActive", null);
        var b = new MemberConfigReference("Name", "FullName", false, "s.IsActive", null);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void MemberConfigReference_DifferentCondition_NotEqual()
    {
        var a = new MemberConfigReference("Name", "FullName", false, "s.IsActive", null);
        var b = new MemberConfigReference("Name", "FullName", false, "s.IsValid", null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_WithAllFields_Equal()
    {
        var a = new MemberConfigReference("Name", "FullName", false, "s.IsActive", "s.IsValid",
            "\"default\"", "Resolver", "MemberResolver");
        var b = new MemberConfigReference("Name", "FullName", false, "s.IsActive", "s.IsValid",
            "\"default\"", "Resolver", "MemberResolver");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void MemberConfigReference_DifferentNullSubstitute_NotEqual()
    {
        var a = new MemberConfigReference("Name", null, false, null, null, "\"default\"", null, null);
        var b = new MemberConfigReference("Name", null, false, null, null, "\"other\"", null, null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_DifferentValueResolver_NotEqual()
    {
        var a = new MemberConfigReference("Name", null, false, null, null, null, "ResolverA", null);
        var b = new MemberConfigReference("Name", null, false, null, null, null, "ResolverB", null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_DifferentMemberValueResolver_NotEqual()
    {
        var a = new MemberConfigReference("Name", null, false, null, null, null, null, "MvrA");
        var b = new MemberConfigReference("Name", null, false, null, null, null, null, "MvrB");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_Null_NotEqual()
    {
        var a = new MemberConfigReference("Name", null, false);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void MemberConfigReference_SameReference_IsEqual()
    {
        var a = new MemberConfigReference("Name", null, false);
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void MemberConfigReference_ObjectEquals()
    {
        var a = new MemberConfigReference("Name", null, false);
        a.Equals((object)new MemberConfigReference("Name", null, false)).Should().BeTrue();
        a.Equals((object)"string").Should().BeFalse();
    }

    // ========================================================================
    // PropertyMatchDescriptor
    // ========================================================================

    [Fact]
    public void PropertyMatchDescriptor_Simple_Equal()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);
        var b = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentConversion_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);
        var b = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.ExplicitCast);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_WithCollection_Equal()
    {
        var a = new PropertyMatchDescriptor("Items", "List<int>", "Items", "List<int>",
            ConversionKind.Collection, "int", "int", CollectionKind.List);
        var b = new PropertyMatchDescriptor("Items", "List<int>", "Items", "List<int>",
            ConversionKind.Collection, "int", "int", CollectionKind.List);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentCollectionKind_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Items", "List<int>", "Items", "int[]",
            ConversionKind.Collection, "int", "int", CollectionKind.List);
        var b = new PropertyMatchDescriptor("Items", "List<int>", "Items", "int[]",
            ConversionKind.Collection, "int", "int", CollectionKind.Array);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_WithInitOnly_Equal()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, null, null, true);
        var b = new PropertyMatchDescriptor("Id", "int", "Id", "int",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, null, null, true);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentInitOnly_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, null, null, true);
        var b = new PropertyMatchDescriptor("Id", "int", "Id", "int",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, null, null, false);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_WithEnumMembers_Equal()
    {
        var members = new EquatableArray<EnumMemberPair>(
            ImmutableArray.Create(new EnumMemberPair("A", "A")));
        var a = new PropertyMatchDescriptor("Status", "SrcEnum", "Status", "DstEnum",
            ConversionKind.EnumByName, null, null, CollectionKind.None, members, null, null);
        var b = new PropertyMatchDescriptor("Status", "SrcEnum", "Status", "DstEnum",
            ConversionKind.EnumByName, null, null, CollectionKind.None, members, null, null);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentDictTypes_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Data", "Dict", "Data", "Dict",
            ConversionKind.Dictionary, null, null, CollectionKind.None,
            null, "string", "int");
        var b = new PropertyMatchDescriptor("Data", "Dict", "Data", "Dict",
            ConversionKind.Dictionary, null, null, CollectionKind.None,
            null, "string", "object");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentCondition_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Name", "string", "Name", "string",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, "cond1", null);
        var b = new PropertyMatchDescriptor("Name", "string", "Name", "string",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, "cond2", null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_DifferentPathIntermediateTypes_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Name", "string", "Addr.City", "string",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, "NS.Address", null);
        var b = new PropertyMatchDescriptor("Name", "string", "Addr.City", "string",
            ConversionKind.Direct, null, null, CollectionKind.None,
            null, null, null, null, null, null, null, "NS.Location", null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_Null_NotEqual()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void PropertyMatchDescriptor_SameReference_IsEqual()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void PropertyMatchDescriptor_ObjectEquals()
    {
        var a = new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct);
        a.Equals((object)new PropertyMatchDescriptor("Id", "int", "Id", "int", ConversionKind.Direct)).Should().BeTrue();
        a.Equals((object)"nope").Should().BeFalse();
    }

    // ========================================================================
    // TypePairDescriptor
    // ========================================================================

    [Fact]
    public void TypePairDescriptor_Simple_Equal()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TypePairDescriptor_DifferentSourceName_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.SrcA", "SrcA", "NS.Dst", "Dst", matches);
        var b = new TypePairDescriptor("NS.SrcB", "SrcB", "NS.Dst", "Dst", matches);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_DifferentCyclicRef_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, true, 10);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_DifferentMaxDepth_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 5);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_DifferentAllowNullCollections_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var ctorParams = new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);
        var addlSources = new EquatableArray<string>(ImmutableArray<string>.Empty);

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, false, false, addlSources, null);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, true, false, addlSources, null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_DifferentDeepClone_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var ctorParams = new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);
        var addlSources = new EquatableArray<string>(ImmutableArray<string>.Empty);

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, false, false, addlSources, null);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, false, true, addlSources, null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_DifferentMappingName_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var ctorParams = new EquatableArray<ConstructorParamDescriptor>(ImmutableArray<ConstructorParamDescriptor>.Empty);
        var addlSources = new EquatableArray<string>(ImmutableArray<string>.Empty);

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, false, false, addlSources, "Profile1");
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams, null, false, false, addlSources, "Profile2");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_WithExpressions_Equal()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, "before", "after", "construct", "convert");
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, "before", "after", "construct", "convert");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TypePairDescriptor_DifferentBeforeMap_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, "beforeA", null, null, null);
        var b = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, "beforeB", null, null, null);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_Null_NotEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_SameReference_IsEqual()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void TypePairDescriptor_ObjectEquals()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);
        a.Equals((object)new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches)).Should().BeTrue();
        a.Equals((object)"wrong").Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_HasConstructorMapping_WithCtorParams()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(ImmutableArray<IncludedTypeDescriptor>.Empty);
        var ctorParams = new EquatableArray<ConstructorParamDescriptor>(
            ImmutableArray.Create(new ConstructorParamDescriptor("id", "int", "Id", "int", ConversionKind.Direct)));

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10,
            false, null, null, null, null, ctorParams);

        a.HasConstructorMapping.Should().BeTrue();
    }

    [Fact]
    public void TypePairDescriptor_HasConstructorMapping_Empty()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);

        a.HasConstructorMapping.Should().BeFalse();
    }

    [Fact]
    public void TypePairDescriptor_IsPolymorphicBase_WithDerivedTypes()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var derived = new EquatableArray<IncludedTypeDescriptor>(
            ImmutableArray.Create(new IncludedTypeDescriptor("NS.Dog", "Dog", "NS.DogDto", "DogDto")));

        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches, derived, false, 10);

        a.IsPolymorphicBase.Should().BeTrue();
    }

    [Fact]
    public void TypePairDescriptor_IsPolymorphicBase_Empty()
    {
        var matches = new EquatableArray<PropertyMatchDescriptor>(ImmutableArray<PropertyMatchDescriptor>.Empty);
        var a = new TypePairDescriptor("NS.Src", "Src", "NS.Dst", "Dst", matches);

        a.IsPolymorphicBase.Should().BeFalse();
    }

    // ========================================================================
    // EquatableArray
    // ========================================================================

    [Fact]
    public void EquatableArray_Equal_Instances_AreEqual()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("a", "b"));
        var b = new EquatableArray<string>(ImmutableArray.Create("a", "b"));

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EquatableArray_DifferentLength_NotEqual()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("a"));
        var b = new EquatableArray<string>(ImmutableArray.Create("a", "b"));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EquatableArray_DifferentElements_NotEqual()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("a", "b"));
        var b = new EquatableArray<string>(ImmutableArray.Create("a", "c"));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EquatableArray_Empty_AreEqual()
    {
        var a = new EquatableArray<string>(ImmutableArray<string>.Empty);
        var b = new EquatableArray<string>(ImmutableArray<string>.Empty);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EquatableArray_DefaultArray_LengthIsZero()
    {
        var a = new EquatableArray<string>(ImmutableArray<string>.Empty);
        a.Length.Should().Be(0);
    }

    [Fact]
    public void EquatableArray_Indexer_ReturnsCorrectElement()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("x", "y"));
        a[0].Should().Be("x");
        a[1].Should().Be("y");
    }

    [Fact]
    public void EquatableArray_Operators()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("a"));
        var b = new EquatableArray<string>(ImmutableArray.Create("a"));
        var c = new EquatableArray<string>(ImmutableArray.Create("b"));

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
    }

    [Fact]
    public void EquatableArray_ObjectEquals_WithBoxedStruct()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("a"));
        a.Equals((object)new EquatableArray<string>(ImmutableArray.Create("a"))).Should().BeTrue();
        a.Equals((object)"wrong type").Should().BeFalse();
    }

    [Fact]
    public void EquatableArray_Enumerable_ReturnsAllElements()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("x", "y", "z"));
        var items = new List<string>();
        foreach (var item in a)
        {
            items.Add(item);
        }
        items.Should().HaveCount(3);
        items[0].Should().Be("x");
        items[1].Should().Be("y");
        items[2].Should().Be("z");
    }

    [Fact]
    public void EquatableArray_IEnumerable_ReturnsAllElements()
    {
        IEnumerable<string> a = new EquatableArray<string>(ImmutableArray.Create("a", "b"));
        var list = a.ToList();
        list.Should().HaveCount(2);
        list[0].Should().Be("a");
        list[1].Should().Be("b");
    }

    [Fact]
    public void EquatableArray_ConstructFromEnumerable()
    {
        var list = new List<string> { "hello", "world" };
        var a = new EquatableArray<string>(list);
        a.Length.Should().Be(2);
        a[0].Should().Be("hello");
        a[1].Should().Be("world");
    }

    [Fact]
    public void EquatableArray_AsImmutableArray_ReturnsCorrectArray()
    {
        var a = new EquatableArray<string>(ImmutableArray.Create("test"));
        var arr = a.AsImmutableArray();
        arr.Length.Should().Be(1);
        arr[0].Should().Be("test");
    }

    // ========================================================================
    // TypePairReference
    // ========================================================================

    [Fact]
    public void TypePairReference_Equal_Instances_AreEqual()
    {
        var a = new TypePairReference("NS.Src", "NS.Dst");
        var b = new TypePairReference("NS.Src", "NS.Dst");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TypePairReference_DifferentSource_NotEqual()
    {
        var a = new TypePairReference("NS.SrcA", "NS.Dst");
        var b = new TypePairReference("NS.SrcB", "NS.Dst");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypePairReference_Null_NotEqual()
    {
        var a = new TypePairReference("NS.Src", "NS.Dst");
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void TypePairReference_SameReference_IsEqual()
    {
        var a = new TypePairReference("NS.Src", "NS.Dst");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void TypePairReference_ObjectEquals()
    {
        var a = new TypePairReference("NS.Src", "NS.Dst");
        a.Equals((object)new TypePairReference("NS.Src", "NS.Dst")).Should().BeTrue();
        a.Equals((object)42).Should().BeFalse();
    }

    [Fact]
    public void TypePairReference_WithAllFields_Equal()
    {
        var members = new EquatableArray<MemberConfigReference>(ImmutableArray<MemberConfigReference>.Empty);
        var derived = new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty);
        var bases = new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty);
        var ctorParams = new EquatableArray<CtorParamConfigReference>(ImmutableArray<CtorParamConfigReference>.Empty);
        var includedMembers = new EquatableArray<string>(ImmutableArray<string>.Empty);
        var addlSources = new EquatableArray<string>(ImmutableArray<string>.Empty);

        var a = new TypePairReference("NS.Src", "NS.Dst", members, derived, bases, 10,
            false, "before", "after", "construct", "convert", ctorParams,
            null, includedMembers, null, false, addlSources, "name");
        var b = new TypePairReference("NS.Src", "NS.Dst", members, derived, bases, 10,
            false, "before", "after", "construct", "convert", ctorParams,
            null, includedMembers, null, false, addlSources, "name");

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TypePairReference_DifferentProjection_NotEqual()
    {
        var members = new EquatableArray<MemberConfigReference>(ImmutableArray<MemberConfigReference>.Empty);
        var derived = new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty);
        var bases = new EquatableArray<IncludedTypeReference>(ImmutableArray<IncludedTypeReference>.Empty);

        var a = new TypePairReference("NS.Src", "NS.Dst", members, derived, bases, null,
            false, null, null, null, null);
        var b = new TypePairReference("NS.Src", "NS.Dst", members, derived, bases, null,
            true, null, null, null, null);

        a.Equals(b).Should().BeFalse();
    }

    // ========================================================================
    // ProfileInfo
    // ========================================================================

    [Fact]
    public void ProfileInfo_Equal_Instances_AreEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);
        var b = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ProfileInfo_DifferentClassName_NotEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("ProfileA", "NS", pairs, "test.cs", 1);
        var b = new ProfileInfo("ProfileB", "NS", pairs, "test.cs", 1);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ProfileInfo_DifferentLine_NotEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);
        var b = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 2);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ProfileInfo_DifferentAllowNullCollections_NotEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var prefixes = new EquatableArray<string>(ImmutableArray<string>.Empty);
        var postfixes = new EquatableArray<string>(ImmutableArray<string>.Empty);

        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1, prefixes, postfixes, false);
        var b = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1, prefixes, postfixes, true);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ProfileInfo_Null_NotEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ProfileInfo_SameReference_IsEqual()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void ProfileInfo_ObjectEquals()
    {
        var pairs = new EquatableArray<TypePairReference>(ImmutableArray<TypePairReference>.Empty);
        var a = new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1);
        a.Equals((object)new ProfileInfo("TestProfile", "NS", pairs, "test.cs", 1)).Should().BeTrue();
        a.Equals((object)"wrong").Should().BeFalse();
    }
}
