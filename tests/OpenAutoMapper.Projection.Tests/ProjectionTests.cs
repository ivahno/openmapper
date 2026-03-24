using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace OpenAutoMapper.Projection.Tests;

public sealed class ProjectionTests : IDisposable
{
    private readonly TestDbContext _db;

    public ProjectionTests()
    {
        _db = new TestDbContext();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        Seed();
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Seed()
    {
        var alice = new Customer
        {
            Name = "Alice",
            Email = "alice@test.com",
            Address = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }
        };
        _db.Customers.Add(alice);
        _db.SaveChanges();

        _db.Orders.Add(new Order
        {
            OrderNumber = "ORD-001",
            Total = 149.97m,
            CustomerId = alice.Id,
            Lines = new List<OrderLine>
            {
                new() { ProductName = "Widget", Quantity = 3, UnitPrice = 29.99m },
                new() { ProductName = "Gadget", Quantity = 1, UnitPrice = 60.00m }
            }
        });

        _db.Orders.Add(new Order
        {
            OrderNumber = "ORD-002",
            Total = 25.00m,
            CustomerId = alice.Id,
            Lines = new List<OrderLine>
            {
                new() { ProductName = "Cable", Quantity = 5, UnitPrice = 5.00m }
            }
        });

        _db.SaveChanges();
    }

    // ---- Flat projection ----

    [Fact]
    public void ProjectTo_FlatCustomer_ProjectsAllProperties()
    {
        var result = _db.Customers.ProjectToCustomerDto().Single();

        result.Id.Should().BePositive();
        result.Name.Should().Be("Alice");
        result.Email.Should().Be("alice@test.com");
    }

    // ---- Nested object projection ----

    [Fact]
    public void ProjectTo_NestedAddress_ProjectsSubObject()
    {
        var result = _db.Customers.ProjectToCustomerWithAddressDto().Single();

        result.Name.Should().Be("Alice");
        result.Address.Should().NotBeNull();
        result.Address!.City.Should().Be("Springfield");
        result.Address.State.Should().Be("IL");
        result.Address.Street.Should().Be("123 Main St");
        result.Address.Zip.Should().Be("62701");
    }

    // ---- Navigation + ForMember ----

    [Fact]
    public void ProjectTo_OrderSummary_FlattensNavigationProperty()
    {
        var results = _db.Orders
            .OrderBy(o => o.OrderNumber)
            .ProjectToOrderSummaryDto()
            .ToList();

        results.Should().HaveCount(2);

        results[0].OrderNumber.Should().Be("ORD-001");
        results[0].CustomerName.Should().Be("Alice");
        results[0].Total.Should().Be(149.97m);

        results[1].OrderNumber.Should().Be("ORD-002");
        results[1].Total.Should().Be(25.00m);
    }

    // ---- Nested collection projection ----

    [Fact]
    public void ProjectTo_OrderWithLines_ProjectsNestedCollection()
    {
        var result = _db.Orders
            .Where(o => o.OrderNumber == "ORD-001")
            .ProjectToOrderWithLinesDto()
            .Single();

        result.OrderNumber.Should().Be("ORD-001");
        result.Lines.Should().HaveCount(2);
        result.Lines.Should().Contain(l => l.ProductName == "Widget" && l.Quantity == 3);
        result.Lines.Should().Contain(l => l.ProductName == "Gadget" && l.Quantity == 1);
    }

    // ---- Composability with LINQ operators ----

    [Fact]
    public void ProjectTo_WithWhere_FiltersBeforeProjection()
    {
        _db.Orders
            .Where(o => o.Total > 100m)
            .ProjectToOrderSummaryDto()
            .ToList()
            .Should().HaveCount(1);
    }

    [Fact]
    public void ProjectTo_WithOrderByDescending_ReturnsSorted()
    {
        var results = _db.Orders
            .OrderByDescending(o => o.Total)
            .ProjectToOrderSummaryDto()
            .ToList();

        results[0].Total.Should().BeGreaterThan(results[1].Total);
    }

    [Fact]
    public void ProjectTo_WithFirstOrDefault_ReturnsSingle()
    {
        var result = _db.Customers
            .ProjectToCustomerDto()
            .FirstOrDefault(c => c.Name == "Alice");

        result.Should().NotBeNull();
        result!.Email.Should().Be("alice@test.com");
    }

    [Fact]
    public void ProjectTo_WithCount_ReturnsCount()
    {
        _db.Orders.ProjectToOrderSummaryDto().Count().Should().Be(2);
    }

    [Fact]
    public void ProjectTo_WithAny_ReturnsBoolean()
    {
        _db.Orders
            .ProjectToOrderSummaryDto()
            .Any(o => o.CustomerName == "Alice")
            .Should().BeTrue();
    }

    [Fact]
    public void ProjectTo_WithSkipTake_ReturnsPage()
    {
        var results = _db.Orders
            .OrderBy(o => o.Id)
            .ProjectToOrderSummaryDto()
            .Skip(1)
            .Take(1)
            .ToList();

        results.Should().HaveCount(1);
        results[0].OrderNumber.Should().Be("ORD-002");
    }
}
