---
layout: default
title: Configuration
nav_order: 3
---

# Configuration

This guide covers the full configuration API for OpenAutoMapper. All configuration is done at compile time through `MapperConfiguration` and `Profile` classes.

## CreateMap

The foundation of all mapping configuration. Register a mapping between a source and destination type:

```csharp
cfg.CreateMap<Source, Destination>();
```

By default, validation is performed against the destination member list. You can change this:

```csharp
cfg.CreateMap<Source, Destination>(MemberList.Source);      // Validate source members
cfg.CreateMap<Source, Destination>(MemberList.Destination);  // Validate dest members (default)
cfg.CreateMap<Source, Destination>(MemberList.None);         // Skip validation
```

## ForMember

Customize how individual destination members are mapped:

```csharp
CreateMap<Order, OrderDto>()
    .ForMember(d => d.CustomerFullName, opt => opt.MapFrom(s => s.Customer.FirstName + " " + s.Customer.LastName))
    .ForMember(d => d.OrderTotal, opt => opt.MapFrom(s => s.Items.Sum(i => i.Price * i.Quantity)));
```

### MapFrom

Specify a custom source expression for a destination member:

```csharp
.ForMember(d => d.DisplayName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
```

### Ignore

Exclude a destination member from mapping:

```csharp
.ForMember(d => d.InternalId, opt => opt.Ignore())
```

### NullSubstitute

Provide a default value when the source member is null:

```csharp
.ForMember(d => d.Name, opt => opt.NullSubstitute("N/A"))
```

### Condition

Map the member only if a condition is met:

```csharp
.ForMember(d => d.Discount, opt =>
{
    opt.Condition((src, dest) => src.IsPremiumCustomer);
    opt.MapFrom(s => s.Discount);
})
```

### PreCondition

Evaluate a condition on the source object before attempting to map the member:

```csharp
.ForMember(d => d.Bonus, opt =>
{
    opt.PreCondition(src => src.IsEligible);
    opt.MapFrom(s => s.BonusAmount);
})
```

## ForPath

Map to nested destination properties using a path expression:

```csharp
CreateMap<Order, OrderDto>()
    .ForPath(d => d.Customer.Name, opt => opt.MapFrom(s => s.CustomerName))
    .ForPath(d => d.Customer.Email, opt => opt.MapFrom(s => s.ContactEmail));
```

`ForPath` is useful when the destination type has nested objects that you want to populate from a flat source.

## Ignore (Shorthand)

A convenience method to ignore a destination member without using `ForMember`:

```csharp
CreateMap<Source, Dest>()
    .Ignore(d => d.InternalField)
    .Ignore(d => d.ComputedValue);
```

## MapFrom (Shorthand)

A convenience method for simple member-to-member remapping:

```csharp
CreateMap<Source, Dest>()
    .MapFrom(d => d.FullName, s => s.Name);
```

## Condition

Apply a global condition to the entire mapping:

```csharp
CreateMap<Source, Dest>()
    .Condition((src, dest) => src.IsActive);
```

When the condition returns `false`, the mapping returns the default destination value.

## NullSubstitute

Apply a global null-substitute value:

```csharp
CreateMap<Source, Dest>()
    .NullSubstitute(new Dest { Name = "Unknown" });
```

## ConstructUsing

Specify a custom constructor for the destination type:

```csharp
CreateMap<Source, Dest>()
    .ConstructUsing(src => new Dest(src.Id, src.Name));
```

This is useful when the destination type does not have a parameterless constructor.

## BeforeMap and AfterMap

Execute custom logic before or after the mapping occurs:

```csharp
CreateMap<Source, Dest>()
    .BeforeMap((src, dest) =>
    {
        // Runs before property mapping
        Console.WriteLine($"Mapping {src.GetType().Name}...");
    })
    .AfterMap((src, dest) =>
    {
        // Runs after property mapping
        dest.MappedAt = DateTime.UtcNow;
    });
```

## ReverseMap

Create a reverse mapping (destination to source) automatically:

```csharp
CreateMap<Source, Dest>()
    .ReverseMap();
```

This is equivalent to:

```csharp
CreateMap<Source, Dest>();
CreateMap<Dest, Source>();
```

Note that `ForMember` customizations on the forward map are **not** automatically applied to the reverse map. You need to configure the reverse map separately if custom member mappings are needed.

## ConvertUsing

Use a custom type converter for complete control over the mapping:

```csharp
CreateMap<Source, Dest>()
    .ConvertUsing(new CustomConverter());

public class CustomConverter : ITypeConverter<Source, Dest>
{
    public Dest Convert(Source source, Dest destination, ResolutionContext context)
    {
        return new Dest
        {
            Id = source.Id,
            FullName = $"{source.FirstName} {source.LastName}",
            IsValid = source.Status == "Active"
        };
    }
}
```

## Include and IncludeBase (Polymorphism)

Map derived types using inheritance-based configuration:

```csharp
CreateMap<Animal, AnimalDto>()
    .Include<Dog, DogDto>()
    .Include<Cat, CatDto>();

CreateMap<Dog, DogDto>();
CreateMap<Cat, CatDto>();
```

Or configure from the derived type upward:

```csharp
CreateMap<Dog, DogDto>()
    .IncludeBase<Animal, AnimalDto>();
```

See [Advanced Topics](advanced.md) for more details on polymorphism.

## MaxDepth

Limit recursive mapping depth to prevent infinite loops with circular references:

```csharp
CreateMap<Employee, EmployeeDto>()
    .MaxDepth(3);
```

## Profile Configuration

### RecognizePrefixes

Tell the mapper to strip prefixes when matching source members to destination members:

```csharp
public class MyProfile : Profile
{
    public MyProfile()
    {
        RecognizePrefixes("Get", "get_");
        CreateMap<Source, Dest>();
    }
}
```

With this configuration, `source.GetName()` would map to `dest.Name`.

### RecognizePostfixes

Similar to prefixes, but for postfixes:

```csharp
RecognizePostfixes("Dto", "ViewModel");
```

### AddGlobalIgnore

Ignore all properties whose names start with a given string across all maps in the profile:

```csharp
AddGlobalIgnore("Internal");
AddGlobalIgnore("_");
```

## Attributes

OpenAutoMapper supports attribute-based configuration as an alternative to the fluent API.

### [AutoMap]

Mark a destination type with its source type:

```csharp
[AutoMap(typeof(Source))]
public class Dest
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### [IgnoreMap]

Exclude a property from mapping:

```csharp
public class Dest
{
    public int Id { get; set; }

    [IgnoreMap]
    public string InternalCode { get; set; }
}
```

### [MapFrom]

Specify the source property name:

```csharp
public class Dest
{
    [MapFrom("FirstName")]
    public string Name { get; set; }
}
```

### [SensitiveProperty]

Mark a property as sensitive. The generator will emit a diagnostic (OM1011) if this property is mapped without explicit configuration:

```csharp
public class Source
{
    [SensitiveProperty]
    public string SocialSecurityNumber { get; set; }
}
```

## Direct Extension Methods

Every `CreateMap<Source, Dest>()` call generates a public `MapToDest()` extension method on the source type. This method is available for direct invocation without going through the `IMapper` interface:

```csharp
// Via IMapper (AutoMapper-compatible, ~19 ns)
var dto = mapper.Map<Source, Dest>(source);

// Via generated extension method (zero-dispatch, ~14 ns)
var dto = source.MapToDest();
```

Both paths execute identical generated mapping code. The extension method simply skips the `IMapper` virtual dispatch and type-switch. Use it on hot paths where performance matters.

The extension class is placed in the destination type's namespace and named `{Source}To{Dest}MappingExtensions`.

## ForCtorParam

Map values into constructor parameters for immutable or record types:

```csharp
CreateMap<Source, ImmutableDest>()
    .ForCtorParam("name", opt => opt.MapFrom(s => s.FullName))
    .ForCtorParam("age", opt => opt.MapFrom(s => s.Years));
```

This is essential for mapping to types with `init`-only properties or positional records.

## ForAllMembers

Apply a configuration action to every destination member:

```csharp
CreateMap<Source, Dest>()
    .ForAllMembers(opt => opt.Ignore());
```

Useful for ignoring all members by default and then selectively enabling the ones you need.

## IncludeMembers

Flatten child source objects into the destination:

```csharp
CreateMap<Order, OrderFlatDto>()
    .IncludeMembers(s => s.Customer, s => s.ShippingAddress);

CreateMap<Customer, OrderFlatDto>();
CreateMap<Address, OrderFlatDto>();
```

Properties from `Customer` and `Address` are mapped into `OrderFlatDto` by convention.

## Enum Mapping Strategies

Control how enum values are mapped between source and destination:

```csharp
// By value (default) — maps enum members by their integer value
CreateMap<Source, Dest>();

// By name — maps enum members by matching name strings
CreateMap<Source, Dest>()
    .UseEnumMappingStrategy(EnumMappingStrategy.ByName);

// Case-insensitive name matching
CreateMap<Source, Dest>()
    .UseEnumMappingStrategy(EnumMappingStrategy.CaseInsensitive);
```

The generator emits OM1050 when a source enum value has no match in the destination, and OM1051 (informational) when a case-insensitive match is used.

## AllowNullCollections

By default, null source collections are mapped to empty collections. To preserve `null`:

```csharp
// Per-map
CreateMap<Source, Dest>()
    .AllowNullCollections();

// Global (in MapperConfiguration)
var config = new MapperConfiguration(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AddProfile<MyProfile>();
});
```

## Dictionary-to-Object Mapping

Map from `Dictionary<string, object>` to a typed destination:

```csharp
CreateMap<Dictionary<string, object>, Dest>();
```

Dictionary keys are matched to destination property names by convention.

## UseDeepCloning

Enable deep cloning to produce fully independent object graphs:

```csharp
CreateMap<Source, Source>()
    .UseDeepCloning();
```

All nested objects and collections are recursively cloned rather than copied by reference.

## IncludeSource (Multiple Source Mapping)

Merge multiple source types into a single destination type:

```csharp
CreateMap<PrimarySource, Dest>()
    .IncludeSource<SecondarySource>();

CreateMap<SecondarySource, Dest>();
```

When mapping, supply the secondary source via `Map` overloads or items dictionary.

## Named Mappings

Create multiple mapping configurations for the same type pair:

```csharp
CreateMap<Source, Dest>()
    .WithName("Summary")
    .ForMember(d => d.Details, opt => opt.Ignore());

CreateMap<Source, Dest>()
    .WithName("Full");
```

Resolve at runtime by specifying the mapping name.

## Next Steps

- [Collections](collections.md) — mapping lists, arrays, and other collection types
- [Projections](projections.md) — EF Core integration with ProjectTo
- [Advanced Topics](advanced.md) — performance best practices, polymorphism, custom resolvers
- [Diagnostics](diagnostics.md) — understanding compiler warnings and errors
