---
layout: default
title: Migration from AutoMapper
nav_order: 7
---

# Migration from AutoMapper

This guide walks you through migrating an existing AutoMapper-based project to OpenAutoMapper. The API is designed to be a near drop-in replacement, so most migrations require minimal code changes.

## Overview

OpenAutoMapper matches AutoMapper's public API surface:
- `Profile`, `CreateMap<S,D>()`, `ForMember()`, `Ignore()`, `ReverseMap()`
- `IMapper`, `MapperConfiguration`, `IConfigurationProvider`
- `ProjectTo<T>()`, `CreateProjection<S,D>()`
- `services.AddAutoMapper()`

The main difference is that OpenAutoMapper generates all mapping code at compile time using a Roslyn source generator, rather than using runtime reflection.

## Step 1: Replace NuGet Packages

Remove the AutoMapper packages and add OpenAutoMapper:

```bash
# Remove AutoMapper
dotnet remove package AutoMapper
dotnet remove package AutoMapper.Extensions.Microsoft.DependencyInjection

# Add OpenAutoMapper
dotnet add package OpenAutoMapper
dotnet add package OpenAutoMapper.DependencyInjection    # if using DI
```

## Step 2: Update Using Statements

In every file that references AutoMapper, change the namespace:

```diff
- using AutoMapper;
+ using OpenAutoMapper;
```

If you use attributes, the namespace is the same:

```diff
- using AutoMapper.Configuration.Annotations;
+ using OpenAutoMapper.Attributes;
```

## Step 3: Build and Fix Diagnostics

```bash
dotnet build
```

The source generator will analyze your mapping configuration and report any unsupported patterns as OM-series compiler diagnostics. Common ones:

| Diagnostic | Meaning | Fix |
|---|---|---|
| OM1001 | Source type unknown at compile time | Use concrete types instead of `object` or unresolved generics |
| OM1002 | Destination type unknown at compile time | Use concrete types |
| OM1003 | Open generic mapping not supported | Create separate mappings for each concrete generic instantiation |
| OM1004 | Interface target not supported | Map to a concrete class instead of an interface |
| OM1010 | Unmapped destination property | Add `.ForMember(d => d.Prop, opt => opt.Ignore())` or add the property to the source |
| OM1011 | Sensitive property mapped without config | Add explicit `.ForMember()` for `[SensitiveProperty]` members |
| OM1050 | Enum value has no match in destination (by-name) | Add the missing enum value or switch to by-value strategy |
| OM1051 | Case-insensitive enum match used (info) | Informational — verify the match is intentional |

See the [Diagnostics Reference](diagnostics.md) for the full catalog.

## API Compatibility Reference

### Fully Supported (No Changes Required)

| AutoMapper API | OpenAutoMapper | Notes |
|---|---|---|
| `Profile` | `Profile` | Same base class |
| `CreateMap<S,D>()` | `CreateMap<S,D>()` | Same signature |
| `ForMember()` | `ForMember()` | Same signature |
| `ForPath()` | `ForPath()` | Same signature |
| `Ignore()` | `Ignore()` | Same signature |
| `MapFrom()` | `MapFrom()` | Same signature |
| `Condition()` | `Condition()` | Same signature |
| `PreCondition()` | `PreCondition()` | Same signature |
| `NullSubstitute()` | `NullSubstitute()` | Same signature |
| `ConstructUsing()` | `ConstructUsing()` | Same signature |
| `BeforeMap()` / `AfterMap()` | `BeforeMap()` / `AfterMap()` | Same signature |
| `ReverseMap()` | `ReverseMap()` | Same signature |
| `MaxDepth()` | `MaxDepth()` | Same signature |
| `Include<S,D>()` | `Include<S,D>()` | Same signature |
| `IncludeBase<S,D>()` | `IncludeBase<S,D>()` | Same signature |
| `ConvertUsing()` | `ConvertUsing()` | Same signature |
| `IMapper.Map<D>()` | `IMapper.Map<D>()` | Same signature |
| `IMapper.Map<S,D>()` | `IMapper.Map<S,D>()` | Same signature |
| `MapperConfiguration` | `MapperConfiguration` | Same constructor |
| `AssertConfigurationIsValid()` | `AssertConfigurationIsValid()` | Same behavior |
| `CreateProjection<S,D>()` | `CreateProjection<S,D>()` | Same signature |
| `AddAutoMapper()` | `AddAutoMapper()` | Same extension method |
| `AddProfile<T>()` | `AddProfile<T>()` | Same signature |
| `AddMaps()` | `AddMaps()` | Same signature |
| `ForCtorParam()` | `ForCtorParam()` | Same signature |
| `ForAllMembers()` | `ForAllMembers()` | Same signature |
| `IncludeMembers()` | `IncludeMembers()` | Same signature |
| Init-only / record types | Init-only / record types | Mapped via generated code |
| Enum mapping strategies | `UseEnumMappingStrategy()` | ByName, ByValue, CaseInsensitive |
| `AllowNullCollections` | `AllowNullCollections` | Global and per-map |
| Dictionary-to-object mapping | `CreateMap<Dictionary<string,object>, D>()` | Keys matched by name |
| `UseDeepCloning()` | `UseDeepCloning()` | Same signature |
| Named mappings | `WithName()` | Same concept |

### Not Supported (Requires Changes)

| AutoMapper Feature | Reason | Workaround |
|---|---|---|
| Open generic mappings | Generator needs concrete types | Create a mapping for each concrete type pair |
| `CreateMap(Type, Type)` (non-generic) | Generator needs static type info | Use generic `CreateMap<S,D>()` |
| `IMapper.Map(object)` with runtime types | AOT cannot generate code for unknown types | Use generic `Map<S,D>()` |
| `ProjectTo` with runtime parameters | Expression must be fully static | Use parameterized queries instead |
| Custom `INamingConvention` implementations | Not yet implemented | Use `RecognizePrefixes` / `RecognizePostfixes` |

## Migration Example

### Before (AutoMapper)

```csharp
// Startup.cs
using AutoMapper;

services.AddAutoMapper(typeof(Startup));

// MappingProfile.cs
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.Total, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Price)));
    }
}

// OrderService.cs
using AutoMapper;

public class OrderService
{
    private readonly IMapper _mapper;

    public OrderService(IMapper mapper) => _mapper = mapper;

    public OrderDto GetOrder(Order order) => _mapper.Map<OrderDto>(order);
}
```

### After (OpenAutoMapper)

```csharp
// Program.cs
using OpenAutoMapper;

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// MappingProfile.cs
using OpenAutoMapper;    // <-- only this line changed

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.Total, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Price)));
    }
}

// OrderService.cs
using OpenAutoMapper;    // <-- only this line changed

public class OrderService
{
    private readonly IMapper _mapper;

    public OrderService(IMapper mapper) => _mapper = mapper;

    public OrderDto GetOrder(Order order) => _mapper.Map<OrderDto>(order);
}
```

## Performance After Migration

After migrating, your `IMapper.Map<>()` calls will be ~3x faster than AutoMapper out of the box. For additional performance on hot paths, switch to the generated extension methods:

```csharp
// Before (AutoMapper): ~60 ns
var dto = _mapper.Map<OrderDto>(order);

// After (OpenAutoMapper via IMapper): ~19 ns — 3x faster, zero code change
var dto = _mapper.Map<OrderDto>(order);

// After (OpenAutoMapper direct call): ~14 ns — 4x faster, matches hand-written
var dto = order.MapToOrderDto();
```

The direct extension methods are generated for every `CreateMap<S,D>()` call and are available as public methods in the destination type's namespace.

## Testing the Migration

After updating all files, run your existing test suite:

```bash
dotnet test
```

If you have mapping validation tests (`config.AssertConfigurationIsValid()`), they will continue to work as before. The source generator provides the same validation at compile time through diagnostics, but runtime validation is also available for defense in depth.

## Next Steps

- [Getting Started](getting-started.md) — full setup guide
- [Configuration](configuration.md) — complete API reference
- [AOT Guide](aot-guide.md) — Native AOT setup and constraints
- [Diagnostics](diagnostics.md) — understanding OM-series diagnostics
