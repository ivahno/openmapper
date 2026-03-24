using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void CircularRef_SelfRef_Detected()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Employee { public string Name { get; set; } public Employee Manager { get; set; } }
public class EmployeeDto { public string Name { get; set; } public EmployeeDto Manager { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Employee, EmployeeDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Should().Contain(d => d.Id == "OM1030");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_MutualRef_AtoB_BtoA()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class A { public string Name { get; set; } public B Other { get; set; } }
public class ADto { public string Name { get; set; } public BDto Other { get; set; } }
public class B { public string Value { get; set; } public A Back { get; set; } }
public class BDto { public string Value { get; set; } public ADto Back { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<A, ADto>();
        CreateMap<B, BDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Id == "OM1030").Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_ThreeLevelCycle()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class X { public string Name { get; set; } public Y Next { get; set; } }
public class XDto { public string Name { get; set; } public YDto Next { get; set; } }
public class Y { public string Name { get; set; } public Z Next { get; set; } }
public class YDto { public string Name { get; set; } public ZDto Next { get; set; } }
public class Z { public string Name { get; set; } public X Next { get; set; } }
public class ZDto { public string Name { get; set; } public XDto Next { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<X, XDto>();
        CreateMap<Y, YDto>();
        CreateMap<Z, ZDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Id == "OM1030").Should().NotBeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_NoCycle_NoDepthTracking()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Parent { public string Name { get; set; } public Child Child { get; set; } }
public class ParentDto { public string Name { get; set; } public ChildDto Child { get; set; } }
public class Child { public string Value { get; set; } }
public class ChildDto { public string Value { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Parent, ParentDto>();
        CreateMap<Child, ChildDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // No cycle means no depth tracking
        generatedSources.Should().NotContain(s => s.Contains("[ThreadStatic]"));
        diagnostics.Where(d => d.Id == "OM1030").Should().BeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_EmitsThreadStaticField()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("[ThreadStatic]"));
        generatedSources.Should().Contain(s => s.Contains("_depth_"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_EmitsDepthCheck()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains(">= 10"));
        generatedSources.Should().Contain(s => s.Contains("return default!"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_EmitsTryFinally()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("try"));
        generatedSources.Should().Contain(s => s.Contains("finally"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_MaxDepthCustom_InGeneratedCode()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains(">= 5"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_ReturnsDefault_AtMaxDepth()
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
        generatedSources.Should().Contain(s => s.Contains("return default!"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_MixedCyclicAndNonCyclic()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
public class Simple { public string Value { get; set; } }
public class SimpleDto { public string Value { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>();
        CreateMap<Simple, SimpleDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Node should have depth tracking
        var nodeExt = generatedSources.FirstOrDefault(s => s.Contains("NodeToNodeDtoMappingExtensions"));
        nodeExt.Should().NotBeNull();
        nodeExt.Should().Contain("[ThreadStatic]");
        // Simple should NOT have depth tracking
        var simpleExt = generatedSources.FirstOrDefault(s => s.Contains("SimpleToSimpleDtoMappingExtensions"));
        simpleExt.Should().NotBeNull();
        simpleExt.Should().NotContain("[ThreadStatic]");
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_DiagnosticInfo_OM1030()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Self { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Self { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        var om1030 = diagnostics.Where(d => d.Id == "OM1030").ToList();
        om1030.Should().NotBeEmpty();
        om1030[0].Severity.Should().Be(DiagnosticSeverity.Info);
    }

    [Fact]
    public void CircularRef_WithPolymorphicSwitch()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Parent { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Parent { get; set; } }
public class SpecialNode : Node { public int Priority { get; set; } }
public class SpecialNodeDto : NodeDto { public int Priority { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Node, NodeDto>()
            .Include<SpecialNode, SpecialNodeDto>();
        CreateMap<SpecialNode, SpecialNodeDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Should have both polymorphic switch AND depth tracking
        generatedSources.Should().Contain(s => s.Contains("source switch") && s.Contains("[ThreadStatic]"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_Collection_OfSelfRefType()
    {
        var source = @"
using System.Collections.Generic;
using OpenAutoMapper;
namespace TestApp;
public class TreeNode { public string Name { get; set; } public List<TreeNode> Children { get; set; } }
public class TreeNodeDto { public string Name { get; set; } public List<TreeNodeDto> Children { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<TreeNode, TreeNodeDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        diagnostics.Where(d => d.Id == "OM1030").Should().NotBeEmpty();
        generatedSources.Should().Contain(s => s.Contains("[ThreadStatic]"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_GeneratedCode_Compiles()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Employee { public string Name { get; set; } public Employee Manager { get; set; } }
public class EmployeeDto { public string Name { get; set; } public EmployeeDto Manager { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Employee, EmployeeDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().NotBeEmpty();
        // Verify no CS compilation errors in generated code
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id.StartsWith("CS", StringComparison.Ordinal)).Should().BeEmpty();
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CircularRef_DepthFieldName_Sanitized()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Node { public string Name { get; set; } public Node Self { get; set; } }
public class NodeDto { public string Name { get; set; } public NodeDto Self { get; set; } }
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
        generatedSources.Should().Contain(s => s.Contains("_depth_NodeToNodeDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
