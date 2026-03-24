---
layout: default
title: Advanced Topics
nav_order: 6
---

# Advanced Topics

This guide covers polymorphic mapping, circular reference handling, MaxDepth, custom value resolvers, type converters, and mapping actions.

## Polymorphic Mapping

OpenAutoMapper supports mapping inheritance hierarchies. Configure the base mapping and then include derived types:

### Top-down (Include)

```csharp
public class AnimalProfile : Profile
{
    public AnimalProfile()
    {
        CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>()
            .Include<Cat, CatDto>();

        CreateMap<Dog, DogDto>();
        CreateMap<Cat, CatDto>();
    }
}
```

When you map an `Animal` that is actually a `Dog`, OpenAutoMapper selects the `Dog -> DogDto` mapping automatically:

```csharp
Animal animal = new Dog { Name = "Rex", Breed = "German Shepherd" };
AnimalDto dto = mapper.Map<AnimalDto>(animal);
// dto is actually a DogDto with Breed populated
```

### Bottom-up (IncludeBase)

```csharp
CreateMap<Dog, DogDto>()
    .IncludeBase<Animal, AnimalDto>();
```

This tells the mapper that when mapping `Dog -> DogDto`, also apply the `Animal -> AnimalDto` base configuration.

### How it works under AOT

The source generator emits a type-checking chain (using `is` pattern matching) in the base mapper method. No reflection is involved:

```csharp
// Generated code (simplified)
public AnimalDto Map(Animal source)
{
    if (source is Dog dog) return Map_Dog_DogDto(dog);
    if (source is Cat cat) return Map_Cat_CatDto(cat);
    return Map_Animal_AnimalDto(source);
}
```

## Circular References

When source types reference each other (e.g., `Employee.Manager` is also an `Employee`), the mapper can enter an infinite loop. OpenAutoMapper handles this with `MaxDepth`.

### MaxDepth

Limit the recursion depth for a mapping:

```csharp
CreateMap<Employee, EmployeeDto>()
    .MaxDepth(3);
```

With `MaxDepth(3)`:
- Level 1: `Employee` is mapped
- Level 2: `Employee.Manager` is mapped
- Level 3: `Employee.Manager.Manager` is mapped
- Level 4+: `Manager` is set to `null`

### Example

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Employee? Manager { get; set; }
    public List<Employee> DirectReports { get; set; } = new();
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EmployeeDto? Manager { get; set; }
    public List<EmployeeDto> DirectReports { get; set; } = new();
}

CreateMap<Employee, EmployeeDto>()
    .MaxDepth(2);
```

## Custom Value Resolvers

For complex mapping logic that cannot be expressed as a simple lambda, use `IValueResolver`:

### Define the resolver

```csharp
public class FullNameResolver : IValueResolver<Customer, CustomerDto, string>
{
    public string Resolve(Customer source, CustomerDto destination, string destMember, ResolutionContext context)
    {
        return $"{source.Title} {source.FirstName} {source.LastName}".Trim();
    }
}
```

### Register the resolver

```csharp
CreateMap<Customer, CustomerDto>()
    .ForMember(d => d.FullName, opt => opt.MapFrom<FullNameResolver>());
```

### Member value resolvers

For resolvers that need access to the specific source member:

```csharp
public class CurrencyResolver : IMemberValueResolver<Order, OrderDto, decimal, string>
{
    public string Resolve(Order source, OrderDto destination, decimal sourceMember, string destMember, ResolutionContext context)
    {
        return $"${sourceMember:F2}";
    }
}
```

### AOT considerations

Value resolvers must be concrete, non-generic classes. The source generator instantiates them at compile time, so they must have a parameterless constructor (or be injected via the service constructor).

## Custom Value Converters

Value converters transform a value without needing access to the full source/destination objects:

```csharp
public class DateTimeToStringConverter : IValueConverter<DateTime, string>
{
    public string Convert(DateTime sourceMember, ResolutionContext context)
    {
        return sourceMember.ToString("yyyy-MM-dd");
    }
}

CreateMap<Event, EventDto>()
    .ForMember(d => d.DateString, opt => opt.ConvertUsing(new DateTimeToStringConverter(), s => s.EventDate));
```

## Custom Type Converters

For complete control over how one type is converted to another:

```csharp
public class OrderToSummaryConverter : ITypeConverter<Order, OrderSummary>
{
    public OrderSummary Convert(Order source, OrderSummary destination, ResolutionContext context)
    {
        return new OrderSummary
        {
            OrderId = source.Id,
            TotalItems = source.Lines.Count,
            GrandTotal = source.Lines.Sum(l => l.Quantity * l.UnitPrice),
            Status = source.IsCancelled ? "Cancelled" : "Active"
        };
    }
}

CreateMap<Order, OrderSummary>()
    .ConvertUsing(new OrderToSummaryConverter());
```

When `ConvertUsing` is specified, all other member-level configuration (`ForMember`, `Ignore`, etc.) is bypassed — the converter has full responsibility.

## Mapping Actions (BeforeMap / AfterMap)

Execute custom logic before or after property mapping:

```csharp
CreateMap<AuditableEntity, AuditableDto>()
    .BeforeMap((src, dest) =>
    {
        // Initialize before mapping
        dest.Timestamps = new TimestampInfo();
    })
    .AfterMap((src, dest) =>
    {
        // Post-processing after mapping
        dest.MappedAtUtc = DateTime.UtcNow;
        dest.SourceTypeName = src.GetType().Name;
    });
```

### Using IMappingAction

For reusable, injectable mapping actions:

```csharp
public class SetAuditFields : IMappingAction<AuditableEntity, AuditableDto>
{
    public void Process(AuditableEntity source, AuditableDto destination, ResolutionContext context)
    {
        destination.MappedAtUtc = DateTime.UtcNow;
    }
}
```

## Attributes for Mapping

### [AutoMap] attribute

Mark a destination type to automatically create a mapping:

```csharp
[AutoMap(typeof(Customer))]
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    [IgnoreMap]
    public string Computed { get; set; }
}
```

The source generator detects `[AutoMap]` attributes and creates the mapping configuration automatically, without requiring a Profile.

### [ValueResolver] attribute

Specify a resolver directly on a property:

```csharp
public class CustomerDto
{
    [ValueResolver(typeof(FullNameResolver))]
    public string FullName { get; set; }
}
```

## Constructor Mapping

For immutable types without parameterless constructors, OpenAutoMapper automatically detects constructors whose parameters match source properties by name (case-insensitive):

```csharp
public record OrderDto(int Id, string CustomerName, decimal Total);

// Auto-detected: ctor params (id, customerName, total) match source properties
CreateMap<Order, OrderDto>();
```

For explicit control, use `ForCtorParam()`:

```csharp
CreateMap<Order, OrderDto>()
    .ForCtorParam("id", opt => opt.MapFrom(s => s.OrderId))
    .ForCtorParam("customerName", opt => opt.MapFrom(s => s.Customer.Name));
```

Init-only properties are mapped via object initializer syntax. Constructor parameters, init-only properties, and mutable properties are all handled in a single hybrid emission:

```csharp
// Generated code (simplified)
var result = new Dest(source.Id, source.Name)  // ctor args
{
    InitProp = source.InitProp,                // init-only in initializer
};
result.MutableProp = source.MutableProp;       // mutable via assignment
```

## Deep Cloning

When source and destination are the same type, `UseDeepCloning()` generates recursive value copies:

```csharp
CreateMap<Order, Order>()
    .UseDeepCloning();

CreateMap<OrderLine, OrderLine>()
    .UseDeepCloning();
```

The generated code recursively clones nested objects and collections. Value types and strings are copied directly. Circular references are handled via the existing `MaxDepth` mechanism.

## Multiple Source Mapping

Merge properties from multiple source objects using `IncludeSource<T>()`:

```csharp
CreateMap<Order, OrderDto>()
    .IncludeSource<Customer>();
```

The generator emits a `ValueTuple`-based extension method:

```csharp
(order, customer).MapToOrderDto();
```

## Named Mappings

Create multiple mapping configurations for the same type pair:

```csharp
CreateMap<Order, OrderDto>();
CreateMap<Order, OrderDto>("summary")
    .ForMember(d => d.Details, opt => opt.Ignore());
```

Named mappings generate separate extension methods with a name suffix:

```csharp
source.MapToOrderDto();          // default mapping
source.MapToOrderDto_summary();  // named mapping
```

## Performance Best Practices

OpenAutoMapper generates the same code a developer would write by hand. To extract maximum performance, follow these guidelines.

### Use direct extension methods on hot paths

The source generator emits a public `MapTo{Dest}()` extension method for each mapping. Calling it directly avoids the `IMapper` interface dispatch (~5 ns overhead):

```csharp
// Hot path — direct call, zero dispatch overhead
// Matches hand-written performance (~14 ns for a 10-property flat DTO)
var dto = source.MapToOrderDto();

// Convenience path — IMapper interface dispatch
// ~19 ns due to virtual call + type-switch
var dto = mapper.Map<OrderDto>(source);
```

Use `IMapper` when you need DI, runtime polymorphism, or AutoMapper API compatibility. Use the direct extension method in tight loops, request handlers, and anywhere throughput matters.

### Prefer the two-generic Map overload

When using `IMapper`, the two-generic overload avoids an extra type check:

```csharp
// Slightly faster — both types known, one fewer runtime check
var dto = mapper.Map<Order, OrderDto>(order);

// Marginally slower — source type resolved at runtime
var dto = mapper.Map<OrderDto>(order);
```

### Seal your DTOs

Mark destination types as `sealed` when they are not inherited. The .NET 10 JIT aggressively devirtualizes and inlines calls on sealed types:

```csharp
public sealed class OrderDto  // JIT can inline property setters
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
```

### Avoid unnecessary BeforeMap/AfterMap

`BeforeMap` and `AfterMap` force the generator to use a two-pass pattern (construct, then assign). Without them, flat DTOs use a single object-initializer expression, which the JIT can optimize as a single allocation + store sequence:

```csharp
// Fast — single object initializer (one-pass)
CreateMap<Order, OrderDto>();

// Adds ~2-3 ns — forces two-pass (construct, then assign, then AfterMap)
CreateMap<Order, OrderDto>()
    .AfterMap((s, d) => d.MappedAt = DateTime.UtcNow);
```

If you need post-processing, consider doing it outside the mapper instead.

### Collection mapping: batch size matters

For large collections, the generated code uses `.Select(...).ToList()`. On .NET 10, the JIT devirtualizes and stack-allocates the enumerator for array-backed collections. Pre-sizing the destination list can help:

```csharp
// Generated code handles this efficiently, but if you need ultimate control:
var dtos = new List<OrderDto>(orders.Count);
foreach (var order in orders)
    dtos.Add(order.MapToOrderDto());
```

### Benchmark your mappings

Run the built-in competitive benchmarks to measure your specific scenarios:

```bash
# All competitive benchmarks (flat, nested, collection, startup)
dotnet run --project tests/OpenAutoMapper.Benchmarks/ -c Release -- --filter '*Competitive*'

# NativeAOT benchmark
dotnet publish tests/OpenAutoMapper.Benchmarks.Aot/ -c Release -r linux-x64
./tests/OpenAutoMapper.Benchmarks.Aot/bin/Release/net10.0/linux-x64/publish/OpenAutoMapper.Benchmarks.Aot
```

## Next Steps

- [AOT Guide](aot-guide.md) — Native AOT constraints and setup
- [Diagnostics](diagnostics.md) — understanding compile-time diagnostic messages
- [Migration from AutoMapper](migration-from-automapper.md) — migrating from AutoMapper
