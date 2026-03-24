---
layout: default
title: Projections
nav_order: 5
---

# Projections (EF Core Integration)

OpenAutoMapper supports IQueryable projections via `CreateProjection<S,D>()` and typed `ProjectTo{Dest}()` extension methods. The source generator emits `Expression<Func<S,D>>` trees at compile time that EF Core translates directly to SQL — no entity materialization, no N+1 queries, no runtime reflection.

## CreateProjection vs CreateMap

| Feature | `CreateMap` | `CreateProjection` |
|---|---|---|
| In-memory mapping | Yes | No |
| IQueryable ProjectTo | No | Yes |
| ForMember (simple property) | Yes | Yes |
| ForPath | Yes | Yes |
| Ignore | Yes | Yes |
| Nested objects | Yes | Yes |
| Nested collections | Yes | Yes |
| Convention flattening | Yes | Yes |
| BeforeMap / AfterMap | Yes | No |
| ConstructUsing | Yes | No |
| Condition (runtime) | Yes | No |
| MaxDepth | Yes | No |
| MapFrom (navigation chain) | Yes | No (use flattening) |

`CreateProjection` only supports configuration that can be expressed as a compile-time `Expression<Func<S,D>>`. Runtime-only features like `BeforeMap`, `AfterMap`, and `ConstructUsing` are not available.

## Quick Example

```csharp
// Entities
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Address? Address { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Address
{
    public int Id { get; set; }
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// DTOs
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AddressDto? Address { get; set; }    // Nested object
}

public class AddressDto
{
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";  // Flattened by convention
}

public class OrderDetailDto
{
    public string OrderNumber { get; set; } = "";
    public List<OrderLineDto> Lines { get; set; } = new();  // Nested collection
}

public class OrderLineDto
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

### Profile

```csharp
public class AppProfile : Profile
{
    public AppProfile()
    {
        // Nested object projection
        CreateProjection<Customer, CustomerDto>();
        CreateProjection<Address, AddressDto>();

        // Flattening: CustomerName auto-maps from Customer.Name
        CreateProjection<Order, OrderSummaryDto>();

        // Nested collection projection
        CreateProjection<Order, OrderDetailDto>();
        CreateProjection<OrderLine, OrderLineDto>();
    }
}
```

### Queries

```csharp
// Nested object: Customer with Address sub-object
var customers = db.Customers.ProjectToCustomerDto().ToList();
// Each customer has Address.City and Address.Zip populated

// Flattening: Order.Customer.Name → CustomerName
var summaries = db.Orders
    .Where(o => o.Total > 100m)
    .OrderByDescending(o => o.Total)
    .ProjectToOrderSummaryDto()
    .Take(20)
    .ToList();

// Nested collection: Order with List<OrderLineDto>
var details = db.Orders
    .Where(o => o.OrderNumber == "ORD-001")
    .ProjectToOrderDetailDto()
    .Single();
// details.Lines contains projected OrderLineDto objects
```

All standard LINQ operators (`Where`, `OrderBy`, `Skip`, `Take`, `FirstOrDefault`, `Count`, `Any`) compose naturally with `ProjectTo` — they translate to SQL.

## Convention-Based Flattening

The generator automatically flattens nested navigation properties by matching DTO property names to navigation chains:

| DTO Property | Source Path | Convention |
|---|---|---|
| `CustomerName` | `Order.Customer.Name` | NavigationProperty + SubProperty |
| `AddressCity` | `Customer.Address.City` | NavigationProperty + SubProperty |
| `CustomerEmail` | `Order.Customer.Email` | NavigationProperty + SubProperty |

No `ForMember` configuration is needed — just name your DTO properties using the convention.

```csharp
// This "just works" — no ForMember needed
public class OrderDto
{
    public string CustomerName { get; set; } = "";   // → Order.Customer.Name
    public string CustomerEmail { get; set; } = "";  // → Order.Customer.Email
}

CreateProjection<Order, OrderDto>();
```

## Nested Object Projections

When a DTO property is a complex type with its own mapping, the generator inlines a nested member initializer:

```csharp
CreateProjection<Customer, CustomerDto>();  // Has Address property
CreateProjection<Address, AddressDto>();    // Mapped separately
```

Generated expression (simplified):

```csharp
source => new CustomerDto
{
    Id = source.Id,
    Name = source.Name,
    Address = source.Address != null ? new AddressDto
    {
        City = source.Address.City,
        Zip = source.Address.Zip
    } : null
}
```

## Nested Collection Projections

Collections of mapped types are projected with `.Select()`:

```csharp
CreateProjection<Order, OrderDetailDto>();    // Has List<OrderLineDto> Lines
CreateProjection<OrderLine, OrderLineDto>();  // Element projection
```

Generated expression (simplified):

```csharp
source => new OrderDetailDto
{
    OrderNumber = source.OrderNumber,
    Lines = source.Lines.Select(x => new OrderLineDto
    {
        ProductName = x.ProductName,
        Quantity = x.Quantity,
        UnitPrice = x.UnitPrice
    }).ToList()
}
```

EF Core translates this to a subquery or JOIN as appropriate.

## Ignoring Properties

```csharp
CreateProjection<Order, OrderDto>()
    .Ignore(d => d.InternalNotes);
```

Ignored properties will not appear in the generated expression or the SQL SELECT clause.

## Injecting IConfigurationProvider

In DI scenarios, inject `IConfigurationProvider` rather than `IMapper` when you only need projections:

```csharp
builder.Services.AddAutoMapper(typeof(AppProfile).Assembly);

public class ReportService
{
    private readonly IConfigurationProvider _config;

    public ReportService(IConfigurationProvider config) => _config = config;

    public IQueryable<ReportDto> GetReports(IQueryable<Report> query)
        => query.ProjectTo<ReportDto>(_config);
}
```

## Limitations

### What works

- Simple property-to-property mapping (same name, compatible types)
- Convention-based flattening (DTO `CustomerName` from `Customer.Name`)
- Nested object projections (with separate `CreateProjection` for the sub-type)
- Nested collection projections (with `.Select()`)
- `ForMember` with simple property remapping (`opt.MapFrom(s => s.OtherProp)`)
- `ForPath` for nested destination paths
- `Ignore` to exclude properties
- Enum-to-enum casting
- Nullable handling
- All LINQ operators (Where, OrderBy, Skip, Take, etc.)

### What does not work in projections

| Feature | Reason | Alternative |
|---|---|---|
| `MapFrom(s => s.Nav.Prop)` | Navigation chains in `MapFrom` are not translated into the generated expression tree | Use convention flattening: name the DTO property `NavProp` |
| `MapFrom(s => s.Items.Count)` | Complex expressions are not inlined into expression trees | Add a computed property to the entity or use a SQL view |
| `BeforeMap` / `AfterMap` | Cannot be translated to SQL | Post-process after `ToList()` |
| `ConstructUsing` | Cannot be translated to SQL | Use default constructor |
| `Condition` / `PreCondition` | Cannot be translated to SQL | Use `.Where()` in LINQ |
| `ConvertUsing` | Type converters are not supported in projections | Use `ForMember` with `MapFrom` |
| `MaxDepth` | Projection expressions are not recursive | Limit navigation depth in queries |
| Dictionary properties | Not translatable to SQL | Emit diagnostic OM1042, skipped |

### Why navigation-chain `MapFrom` is not supported

The source generator emits `Expression<Func<S,D>>` at compile time. When you write:

```csharp
.ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
```

The generator sees the `MapFrom` lambda but only extracts the final member name (`"Name"`). It cannot resolve the full navigation chain (`Customer.Name`) on the source type because it doesn't trace through navigation properties at compile time.

**Use convention-based flattening instead** — it is fully supported and requires zero configuration:

```csharp
// Instead of ForMember + MapFrom:
public class OrderDto
{
    public string CustomerName { get; set; } = "";  // Auto-maps from Customer.Name
}

CreateProjection<Order, OrderDto>();  // No ForMember needed
```

## Sample

See the [EF Core sample](../samples/OpenAutoMapper.Samples.EfCore/) for a complete working example with SQLite, including nested objects, nested collections, and convention-based flattening.

## Next Steps

- [Configuration](configuration.md) — full configuration API reference
- [Collections](collections.md) — collection mapping for in-memory scenarios
- [Advanced](advanced.md) — performance best practices, polymorphism, custom resolvers
