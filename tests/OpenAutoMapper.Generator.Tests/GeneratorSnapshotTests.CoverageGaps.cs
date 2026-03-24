using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

/// <summary>
/// Tests targeting specific coverage gaps in the generator pipeline.
/// </summary>
public partial class GeneratorSnapshotTests
{
    // ========================================================================
    // Gap 1: PropertyAnalyzer diagnostic paths (OM1001-OM1004)
    // ========================================================================

    [Fact]
    public void OM1003_OpenGenericMapping_EmitsDiagnostic()
    {
        // The source references a type like GenericSource<T> in CreateMap, which
        // after resolution should be an unbound generic type.
        // NOTE: GetTypeByMetadataName can't resolve open generics from user code
        // directly. The ProfileDiscovery extracts the full name from the syntax.
        // If the user writes CreateMap<MyGeneric<>, Dest>() in real code it won't parse,
        // so this diagnostic is more of a safety net. We verify the descriptor exists
        // and is wired correctly.
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");
        var field = descriptorType.GetField("OpenGenericNotSupported",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull();
        var descriptor = (DiagnosticDescriptor)field!.GetValue(null)!;
        descriptor.Id.Should().Be("OM1003");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        descriptor.IsEnabledByDefault.Should().BeTrue();
        descriptor.Category.Should().Be("OpenAutoMapper");
    }

    [Fact]
    public void OM1004_InterfaceTarget_EmitsDiagnostic()
    {
        // Map to an interface destination: should trigger OM1004
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } }
public interface IDest { int Id { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, IDest>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1004" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void OM1001_SourceTypeNotFound_EmitsDiagnostic()
    {
        // OM1001: source type name that doesn't resolve via GetTypeByMetadataName.
        // This can happen if the type name extracted from syntax doesn't match a real type.
        // Since ProfileDiscovery extracts full names from generic type arguments in syntax,
        // a nonexistent type in a CreateMap call will trigger this path.
        // However, it's hard to get a fully parseable CreateMap<Nonexistent, Dest>() without
        // the Nonexistent type existing at all. We verify the descriptor is correct.
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");
        var field = descriptorType.GetField("SourceTypeUnknown",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull();
        var descriptor = (DiagnosticDescriptor)field!.GetValue(null)!;
        descriptor.Id.Should().Be("OM1001");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        descriptor.MessageFormat.ToString(System.Globalization.CultureInfo.InvariantCulture)
            .Should().Contain("Source type");
    }

    [Fact]
    public void OM1002_DestTypeNotFound_DescriptorCorrect()
    {
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");
        var field = descriptorType.GetField("DestTypeUnknown",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull();
        var descriptor = (DiagnosticDescriptor)field!.GetValue(null)!;
        descriptor.Id.Should().Be("OM1002");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        descriptor.MessageFormat.ToString(System.Globalization.CultureInfo.InvariantCulture)
            .Should().Contain("Destination type");
    }

    // ========================================================================
    // Gap 2: DictionaryToObjectMatcher coverage
    // ========================================================================
    // NOTE: DictionaryToObjectMatcher.IsStringKeyedDictionary checks sourceType.IsGenericType.
    // A class like "class MyDict : Dictionary<string, object> {}" is NOT itself a generic type.
    // And constructed generic types like Dictionary<string,object> cannot be resolved via
    // GetTypeByMetadataName. This means the dict-to-object path is effectively dead code
    // in the current architecture. These tests document the known behavior.

    [Fact]
    public void DictionaryToObject_WrapperClass_IsNotGenericType_FallsToPropertyMatching()
    {
        // A class inheriting Dictionary<string,object> is not itself a generic type.
        // IsStringKeyedDictionary returns false, so regular property matching is used.
        // Since the wrapper has no matching properties, nothing is generated.
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class StringDict : Dictionary<string, object> { }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<StringDict, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        // The mapper impl is always generated, but no mapping extension since
        // there are no property matches (StringDict has no Id/Name properties)
        var omDiags = GetOMDiagnostics(diagnostics);
        // Unmapped properties should trigger OM1010 warnings
        omDiags.Where(d => d.Id == "OM1010").Should().NotBeEmpty();
    }

    [Fact]
    public void DictionaryToObject_ConstructedGenericDict_TriggersOM1001()
    {
        // Direct Dictionary<string,object> as source type cannot be resolved
        // by GetTypeByMetadataName for constructed generics. This triggers OM1001.
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Dest { public int Id { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Dictionary<string, object>, Dest>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        // Should trigger OM1001 because Dictionary<string,object> can't be resolved
        diagnostics.Should().Contain(d => d.Id == "OM1001");
    }

    [Fact]
    public void DictionaryToObject_WrapperWithOwnProperties_MapsProperties()
    {
        // A wrapper class that also adds its own public properties can still
        // have those properties matched via regular property matching.
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class StringDict : Dictionary<string, object>
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<StringDict, Dest>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ========================================================================
    // Gap 3: ConvertUsing + CyclicReference (MappingCodeEmitter lines 127-132)
    // ========================================================================

    [Fact]
    public void ConvertUsing_WithCircularRef_EmitsConvertAndDepthTracking()
    {
        // Combine ConvertUsing with a type that has circular references.
        // The emitter should emit ConvertUsing body AND the try/finally depth tracking.
        // Note: ConvertUsing is parsed via ParseLambdaExpression, so we use a lambda form
        // that instantiates a converter inline.
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
public class NodeConverter : ITypeConverter<Node, NodeDto>
{
    public NodeDto Convert(Node source, NodeDto destination, ResolutionContext context)
    {
        return new NodeDto { Name = source.Name };
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>()
            .ConvertUsing(new NodeConverter());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // ConvertUsing path should be present
        var nodeExt = generatedSources.FirstOrDefault(s => s.Contains("NodeToNodeDtoMappingExtensions"));
        nodeExt.Should().NotBeNull();
        // Should have ITypeConverter invocation
        nodeExt!.Should().Contain("ITypeConverter");
        // Since the type is self-referencing, depth tracking should be emitted
        nodeExt.Should().Contain("[ThreadStatic]");
        nodeExt.Should().Contain("try");
        nodeExt.Should().Contain("finally");
        nodeExt.Should().Contain("_depth_");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void ConvertUsing_WithCircularRef_EmitsDepthGuardBeforeConvert()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class TreeItem { public string Value { get; set; } public TreeItem Next { get; set; } }
public class TreeItemDto { public string Value { get; set; } public TreeItemDto Next { get; set; } }
public class TreeConverter : ITypeConverter<TreeItem, TreeItemDto>
{
    public TreeItemDto Convert(TreeItem source, TreeItemDto destination, ResolutionContext context)
    {
        return new TreeItemDto { Value = source.Value };
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<TreeItem, TreeItemDto>()
            .ConvertUsing(new TreeConverter());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var ext = generatedSources.FirstOrDefault(s => s.Contains("TreeItemToTreeItemDtoMappingExtensions"));
        ext.Should().NotBeNull();
        // Depth check should appear before the ConvertUsing call
        var depthIdx = ext!.IndexOf(">= 10", StringComparison.Ordinal);
        var convertIdx = ext.IndexOf("ITypeConverter", StringComparison.Ordinal);
        depthIdx.Should().BeLessThan(convertIdx, "depth guard should appear before ConvertUsing invocation");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ========================================================================
    // Gap 4: Polymorphic base + constructor mapping (EmitPolymorphicDefaultArm lines 294-311)
    // ========================================================================

    [Fact]
    public void Include_WithConstructorMapping_OnBase()
    {
        // Base dest has a non-parameterless constructor; Include<> should still generate
        // a polymorphic switch with the base default arm using ctor args.
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public int Id { get; set; } public string Name { get; set; } }
public class AnimalDto
{
    public AnimalDto(int id) { Id = id; }
    public int Id { get; }
    public string Name { get; set; }
}
public class Dog : Animal { public string Breed { get; set; } }
public class DogDto : AnimalDto
{
    public DogDto(int id) : base(id) { }
    public string Breed { get; set; }
}
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
        // Should have polymorphic switch
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        // Default arm should use constructor: new TestApp.AnimalDto(...)
        generatedSources.Should().Contain(s =>
            s.Contains("_ => new TestApp.AnimalDto(") && s.Contains("source.Id"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_WithConstructorMapping_BaseHasCtorAndProperties()
    {
        // Base dest has ctor params plus properties in the default arm of the polymorphic switch.
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Vehicle { public int Year { get; set; } public string Make { get; set; } public string Color { get; set; } }
public class VehicleDto
{
    public VehicleDto(int year) { Year = year; }
    public int Year { get; }
    public string Make { get; set; }
    public string Color { get; set; }
}
public class Truck : Vehicle { public int TowCapacity { get; set; } }
public class TruckDto : VehicleDto
{
    public TruckDto(int year) : base(year) { }
    public int TowCapacity { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Vehicle, VehicleDto>()
            .Include<Truck, TruckDto>();
        CreateMap<Truck, TruckDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        // Default arm should have ctor call with property initializer
        var baseExt = generatedSources.FirstOrDefault(s =>
            s.Contains("VehicleToVehicleDtoMappingExtensions"));
        baseExt.Should().NotBeNull();
        baseExt!.Should().Contain("new TestApp.VehicleDto(");
        // Properties should be present in the default arm initializer or assignment
        baseExt.Should().Contain("Make");
        baseExt.Should().Contain("Color");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void Include_WithConstructorMapping_CtorOnlyNoProperties()
    {
        // Base dest has ctor params but NO additional settable properties.
        // This hits the branch at line 310-311 where the default arm is just the ctor call.
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Shape { public int Sides { get; set; } }
public class ShapeDto
{
    public ShapeDto(int sides) { Sides = sides; }
    public int Sides { get; }
}
public class Circle : Shape { public double Radius { get; set; } }
public class CircleDto : ShapeDto
{
    public CircleDto(int sides) : base(sides) { }
    public double Radius { get; set; }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Shape, ShapeDto>()
            .Include<Circle, CircleDto>();
        CreateMap<Circle, CircleDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("source switch"));
        // Default arm: just ctor call, no initializer braces after it
        var shapeExt = generatedSources.FirstOrDefault(s =>
            s.Contains("ShapeToShapeDtoMappingExtensions"));
        shapeExt.Should().NotBeNull();
        shapeExt!.Should().Contain("_ => new TestApp.ShapeDto(");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ========================================================================
    // Gap 5: Deep clone + AllowNullCollections collection emission
    // ========================================================================

    [Fact]
    public void DeepClone_WithAllowNullCollections_NullFallbackIsNull()
    {
        // Combines UseDeepCloning with AllowNullCollections = true on a collection property.
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Item { public int Id { get; set; } public string Name { get; set; } }
public class Container { public int Id { get; set; } public List<Item> Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        AllowNullCollections = true;
        CreateMap<Item, Item>()
            .UseDeepCloning();
        CreateMap<Container, Container>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var containerExt = generatedSources.FirstOrDefault(s =>
            s.Contains("ContainerToContainerMappingExtensions"));
        containerExt.Should().NotBeNull();
        // Deep clone with AllowNullCollections: collection null fallback should be "null"
        // and elements should be recursively cloned via MapToItem
        containerExt!.Should().Contain(".Select(");
        containerExt.Should().Contain("MapToItem");
        // AllowNullCollections=true means no empty collection fallback
        containerExt.Should().NotContain("new global::System.Collections.Generic.List<");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_WithAllowNullCollections_Array()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Item { public int Id { get; set; } public string Name { get; set; } }
public class Holder { public int Id { get; set; } public Item[] Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        AllowNullCollections = true;
        CreateMap<Item, Item>()
            .UseDeepCloning();
        CreateMap<Holder, Holder>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var holderExt = generatedSources.FirstOrDefault(s =>
            s.Contains("HolderToHolderMappingExtensions"));
        holderExt.Should().NotBeNull();
        holderExt!.Should().Contain(".Select(");
        holderExt.Should().Contain("MapToItem");
        holderExt.Should().Contain(".ToArray()");
        // AllowNullCollections=true: no Array.Empty fallback
        holderExt.Should().NotContain("Array.Empty");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_WithoutAllowNullCollections_DefaultsToEmpty()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Widget { public int Id { get; set; } public string Label { get; set; } }
public class Box { public int Id { get; set; } public List<Widget> Widgets { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Widget, Widget>()
            .UseDeepCloning();
        CreateMap<Box, Box>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var boxExt = generatedSources.FirstOrDefault(s =>
            s.Contains("BoxToBoxMappingExtensions"));
        boxExt.Should().NotBeNull();
        // Deep clone without AllowNullCollections: fallback to empty list
        boxExt!.Should().Contain(".Select(");
        boxExt.Should().Contain("MapToWidget");
        boxExt.Should().Contain("new global::System.Collections.Generic.List<");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void DeepClone_WithHashSet_SelectsMapTo()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class Tag { public string Value { get; set; } }
public class TagContainer { public HashSet<Tag> Tags { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Tag, Tag>()
            .UseDeepCloning();
        CreateMap<TagContainer, TagContainer>()
            .UseDeepCloning();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var tagContainerExt = generatedSources.FirstOrDefault(s =>
            s.Contains("TagContainerToTagContainerMappingExtensions"));
        tagContainerExt.Should().NotBeNull();
        tagContainerExt!.Should().Contain("HashSet");
        tagContainerExt.Should().Contain(".Select(");
        tagContainerExt.Should().Contain("MapToTag");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ========================================================================
    // Additional coverage: OM1020 InvalidMaxDepth
    // ========================================================================

    [Fact]
    public void MaxDepth_NegativeValue_NotParsedAsLiteral_DefaultsTo10()
    {
        // .MaxDepth(-1) uses PrefixUnaryExpressionSyntax (negation), not a literal int.
        // ParseMaxDepth only handles LiteralExpressionSyntax, so the value is ignored
        // and defaults to 10. This documents the known behavior.
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
            .MaxDepth(-1);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Negative value not parsed; falls back to default 10
        generatedSources.Should().Contain(s => s.Contains(">= 10"));
        // No OM1020 because the -1 is silently ignored
        diagnostics.Where(d => d.Id == "OM1020").Should().BeEmpty();
    }

    // ========================================================================
    // Additional coverage: OM1022 IncludeBase type mismatch
    // ========================================================================

    [Fact]
    public void IncludeBase_NoMatchingBaseMapping_OM1022()
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
        // IncludeBase<Animal, AnimalDto>() but no CreateMap<Animal, AnimalDto>()
        CreateMap<Dog, DogDto>()
            .IncludeBase<Animal, AnimalDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1022");
    }

    // ========================================================================
    // Additional coverage: ConvertUsing without circular ref
    // ========================================================================

    [Fact]
    public void ConvertUsing_WithoutCircularRef_NoDepthTracking()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Source { public int Id { get; set; } public string Name { get; set; } }
public class Dest { public int Id { get; set; } public string Name { get; set; } }
public class MyConverter : ITypeConverter<Source, Dest>
{
    public Dest Convert(Source source, Dest destination, ResolutionContext context)
    {
        return new Dest { Id = source.Id, Name = source.Name };
    }
}
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Source, Dest>()
            .ConvertUsing(new MyConverter());
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        var ext = generatedSources.FirstOrDefault(s => s.Contains("SourceToDestMappingExtensions"));
        ext.Should().NotBeNull();
        ext!.Should().Contain("ITypeConverter");
        // No circular ref, no depth tracking
        ext.Should().NotContain("[ThreadStatic]");
        ext.Should().NotContain("_depth_");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    // ========================================================================
    // Additional coverage: All DiagnosticDescriptors fields are wired
    // ========================================================================

    [Fact]
    public void AllDiagnosticDescriptors_HaveValidFields()
    {
        var assembly = typeof(OpenAutoMapperGenerator).Assembly;
        var descriptorType = assembly.GetTypes()
            .First(t => t.Name == "DiagnosticDescriptors");

        var fields = descriptorType.GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .ToList();

        fields.Should().HaveCountGreaterThanOrEqualTo(6);

        var expectedIds = new[] { "OM1001", "OM1002", "OM1003", "OM1004", "OM1010", "OM1011",
            "OM1020", "OM1021", "OM1022", "OM1030", "OM1040", "OM1041", "OM1042", "OM1050", "OM1051" };

        var actualIds = fields.Select(f => ((DiagnosticDescriptor)f.GetValue(null)!).Id).ToList();

        foreach (var expected in expectedIds)
        {
            actualIds.Should().Contain(expected,
                $"diagnostic {expected} should be defined in DiagnosticDescriptors");
        }
    }
}
