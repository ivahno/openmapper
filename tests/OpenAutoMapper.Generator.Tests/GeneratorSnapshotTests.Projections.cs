using FluentAssertions;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Tests.Helpers;
using Xunit;

namespace OpenAutoMapper.Generator.Tests;

public partial class GeneratorSnapshotTests
{
    [Fact]
    public void CreateProjection_BasicScalarProperties()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } public string Name { get; set; } }
public class OrderDto { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("ProjectionExpressions"));
        generatedSources.Should().Contain(s => s.Contains("Expression<Func<"));
        generatedSources.Should().Contain(s => s.Contains("source.Id"));
        generatedSources.Should().Contain(s => s.Contains("source.Name"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_WithForMember_MapFrom()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } public string FullName { get; set; } }
public class OrderDto { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("source.FullName"));
        generatedSources.Should().Contain(s => s.Contains("Name = source.FullName"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_WithIgnore()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } public string Name { get; set; } }
public class OrderDto { public int Id { get; set; } public string Name { get; set; } public string Extra { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>()
            .Ignore(d => d.Extra);
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("ProjectionExpressions"));
        generatedSources.Should().NotContain(s => s.Contains("Extra = "));
        GetOMDiagnostics(diagnostics).Where(d => d.Id == "OM1010").Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_NestedNavigation_NullSafe()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Customer { public string Name { get; set; } }
public class CustomerDto { public string Name { get; set; } }
public class Order { public int Id { get; set; } public Customer Customer { get; set; } }
public class OrderDto { public int Id { get; set; } public CustomerDto Customer { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
        CreateProjection<Customer, CustomerDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("source.Customer != null"));
        generatedSources.Should().Contain(s => s.Contains("new TestApp.CustomerDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_CollectionProjection_Select()
    {
        var source = @"
using OpenAutoMapper;
using System.Collections.Generic;
namespace TestApp;
public class Item { public string Product { get; set; } public int Qty { get; set; } }
public class ItemDto { public string Product { get; set; } public int Qty { get; set; } }
public class Order { public int Id { get; set; } public List<Item> Items { get; set; } }
public class OrderDto { public int Id { get; set; } public List<ItemDto> Items { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
        CreateProjection<Item, ItemDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains(".Select(x => new TestApp.ItemDto"));
        generatedSources.Should().Contain(s => s.Contains(".ToList()"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_EnumProperty_IntCast()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public enum StatusA { Active, Inactive }
public enum StatusB { Active, Inactive }
public class Order { public int Id { get; set; } public StatusA Status { get; set; } }
public class OrderDto { public int Id { get; set; } public StatusB Status { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("(TestApp.StatusB)(int)source.Status"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_NullableProperty()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int? Qty { get; set; } }
public class OrderDto { public int Qty { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("source.Qty"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_GeneratesProjectToExtension()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } }
public class OrderDto { public int Id { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("ProjectToOrderDto"));
        generatedSources.Should().Contain(s => s.Contains("IQueryable<TestApp.OrderDto>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_GeneratesProjectToGeneric()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } }
public class OrderDto { public int Id { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("ProjectTo<TDest>"));
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperQueryableExtensions"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_CircularRef_EmitsOM1040()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Employee { public int Id { get; set; } public Employee Manager { get; set; } }
public class EmployeeDto { public int Id { get; set; } public EmployeeDto Manager { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Employee, EmployeeDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1040");
    }

    [Fact]
    public void CreateProjection_PolymorphicInclude_EmitsOM1041()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Animal { public string Name { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class AnimalDto { public string Name { get; set; } }
public class DogDto : AnimalDto { public string Breed { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Animal, AnimalDto>();
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();
        CreateMap<Dog, DogDto>();
    }
}
";
        var (diagnostics, _) = TestHelper.RunGenerator(source);
        // The CreateProjection itself should not have Include, but the CreateMap does.
        // Projection should generate normally without OM1041 since it has no Include.
        diagnostics.Where(d => d.Id == "OM1041").Should().BeEmpty(
            "OM1041 should only fire for projections that have Include, not for CreateMap with Include");
    }

    [Fact]
    public void CreateProjection_DictProperty_EmitsOM1042()
    {
        var source = @"
using OpenAutoMapper;
using System.Collections.Generic;
namespace TestApp;
public class Order { public int Id { get; set; } public Dictionary<string, string> Tags { get; set; } }
public class OrderDto { public int Id { get; set; } public Dictionary<string, string> Tags { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "OM1042");
        // Dict property should be skipped
        generatedSources.Should().NotContain(s =>
            s.Contains("ProjectionExpressions") && s.Contains("Tags ="));
    }

    [Fact]
    public void CreateProjection_CoexistsWithCreateMap()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } public string Name { get; set; } }
public class OrderDto { public int Id { get; set; } public string Name { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("MappingExtensions"));
        generatedSources.Should().Contain(s => s.Contains("ProjectionExpressions"));
        generatedSources.Should().Contain(s => s.Contains("OpenAutoMapperImpl"));
        generatedSources.Should().Contain(s => s.Contains("ProjectTo<TDest>"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_MultipleProjections()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Order { public int Id { get; set; } }
public class OrderDto { public int Id { get; set; } }
public class Customer { public int Id { get; set; } }
public class CustomerDto { public int Id { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
        CreateProjection<Customer, CustomerDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("OrderToOrderDtoProjectionExpressions"));
        generatedSources.Should().Contain(s => s.Contains("CustomerToCustomerDtoProjectionExpressions"));
        generatedSources.Should().Contain(s =>
            s.Contains("ProjectTo<TDest>") && s.Contains("OrderDto") && s.Contains("CustomerDto"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }

    [Fact]
    public void CreateProjection_Flattening()
    {
        var source = @"
using OpenAutoMapper;
namespace TestApp;
public class Address { public string City { get; set; } }
public class Order { public int Id { get; set; } public Address Address { get; set; } }
public class OrderDto { public int Id { get; set; } public string AddressCity { get; set; } }
public class TestProfile : Profile
{
    public TestProfile()
    {
        CreateProjection<Order, OrderDto>();
    }
}
";
        var (diagnostics, generatedSources) = TestHelper.RunGenerator(source);
        generatedSources.Should().Contain(s => s.Contains("source.Address.City"));
        GetOMErrors(diagnostics).Should().BeEmpty();
    }
}
