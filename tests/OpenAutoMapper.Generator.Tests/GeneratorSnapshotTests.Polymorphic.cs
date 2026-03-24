using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void Include_SingleDerived_GeneratesSwitchExpression()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // The base mapping should have a switch expression
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        generatedSources.Should().Contain(s => s.Contains("Dog"));
        generatedSources.Should().Contain(s => s.Contains("MapToDogDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_MultipleDerived_AllArmsPresent()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class Cat : Animal { public bool Indoor { get; set; } }
public class CatDto : AnimalDto { public bool Indoor { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>()
            .Include<Cat, CatDto>();
        CreateMap<Dog, DogDto>();
        CreateMap<Cat, CatDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDogDto"));
        generatedSources.Should().Contain(s => s.Contains("MapToCatDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_DefaultArm_IsBaseMapping()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Default arm should still create base type
        generatedSources.Should().Contain(s => s.Contains("_ => new TestApp.AnimalDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_WithForMember_CoexistsCorrectly()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string FullName { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_WithReverseMap_CoexistsCorrectly()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>()
            .ReverseMap();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToAnimalDto"));
        generatedSources.Should().Contain(s => s.Contains("MapToAnimal"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_NestedHierarchy_ThreeLevels()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class Puppy : Dog { public int AgeWeeks { get; set; } }
public class PuppyDto : DogDto { public int AgeWeeks { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>()
            .Include<Puppy, PuppyDto>();
        CreateMap<Puppy, PuppyDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("MapToDogDto"));
        generatedSources.Should().Contain(s => s.Contains("MapToPuppyDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_MixedWithScalarProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public int Id { get; set; } public string Name { get; set; } }
public class AnimalDto { public int Id { get; set; } public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_MapperImpl_DerivedBeforeBase()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // In the mapper impl, Dog should appear before Animal in the if-chain
        var mapperImpl = generatedSources.FirstOrDefault(s => s.Contains("OpenAutoMapperImpl"));
        mapperImpl.Should().NotBeNull();
        var dogIdx = mapperImpl!.IndexOf("source is TestApp.Dog", StringComparison.Ordinal);
        var animalIdx = mapperImpl.IndexOf("source is TestApp.Animal", StringComparison.Ordinal);
        dogIdx.Should().BeLessThan(animalIdx, "derived type should appear before base type in dispatch");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_DerivedHasExtraProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } public int Weight { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } public int Weight { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Breed"));
        generatedSources.Should().Contain(s => s.Contains("source.Weight"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IncludeBase_MergesIntoParentSwitch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>();
        CreateMap<Dog, DogDto>()
            .IncludeBase<Animal, AnimalDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // The base mapping for Animal->AnimalDto should now have Dog in its switch
        generatedSources.Should().Contain(s => s.Contains("source switch") && s.Contains("MapToDogDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_GeneratedCode_HasNoOMErrors()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_ParsedFromFluentChain()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Base { public int Id { get; set; } }
public class BaseDto { public int Id { get; set; } }
public class Derived : Base { public string Extra { get; set; } }
public class DerivedDto : BaseDto { public string Extra { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Base, BaseDto>()
            .Include<Derived, DerivedDto>();
        CreateMap<Derived, DerivedDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void IncludeBase_ParsedFromFluentChain()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Base { public int Id { get; set; } }
public class BaseDto { public int Id { get; set; } }
public class Derived : Base { public string Extra { get; set; } }
public class DerivedDto : BaseDto { public string Extra { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Base, BaseDto>();
        CreateMap<Derived, DerivedDto>()
            .IncludeBase<Base, BaseDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_TypeMismatch_OM1021()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class Unrelated { public string Value { get; set; } }
public class UnrelatedDto { public string Value { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Unrelated, UnrelatedDto>();
        CreateMap<Unrelated, UnrelatedDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1021");
    }

    [Fact]
    public void MaxDepth_ParsedFromFluentChain()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Child { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Child { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>()
            .MaxDepth(5);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MaxDepth_DefaultValue10()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Child { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Child { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Default max depth is 10, should see depth check with 10
        generatedSources.Should().Contain(s => s.Contains(">= 10"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MaxDepth_CustomValue()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Child { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Child { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>()
            .MaxDepth(3);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains(">= 3"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void MaxDepth_InvalidValue_OM1020()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Child { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Child { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>()
            .MaxDepth(0);
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1020");
    }

    [Fact]
    public void Include_NoProperties_StillGeneratesSwitch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { }
public class AnimalDto { }
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
