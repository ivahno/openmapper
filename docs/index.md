---
layout: default
title: Home
nav_order: 1
---

# OpenAutoMapper Documentation

Welcome to the OpenAutoMapper documentation. OpenAutoMapper is a Native AOT-first, high-performance object mapper for .NET, powered by Roslyn source generators.

## Why OpenAutoMapper?

Traditional object mappers like AutoMapper rely on runtime reflection and `Expression.Compile()` to generate mapping code. This works well in classic .NET scenarios, but breaks under Native AOT compilation and introduces runtime overhead for configuration and per-map execution.

OpenAutoMapper solves this by shifting all mapping code generation to compile time using a Roslyn incremental source generator. The result is:

- **Zero runtime reflection** — mapping code is plain C# emitted during compilation
- **Full Native AOT compatibility** — works with `PublishAot=true` and `TrimMode=full`
- **Identical API** — if you know AutoMapper, you already know OpenAutoMapper
- **Compile-time diagnostics** — misconfiguration is reported as compiler warnings, not runtime exceptions

## Getting Started

The fastest way to get started is to install the NuGet package and create your first mapping:

```bash
dotnet add package OpenAutoMapper
```

```csharp
using OpenAutoMapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}

// Configure and map
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<UserProfile>();
});

var mapper = config.CreateMapper();
var dto = mapper.Map<UserDto>(user);
```

For a detailed walkthrough, see the [Getting Started guide](getting-started.md).

## Documentation Overview

| Guide | Description |
|---|---|
| [Getting Started](getting-started.md) | Installation, first mapping, Profile setup, DI registration |
| [Configuration](configuration.md) | CreateMap, ForMember, ForPath, Ignore, Condition, and more |
| [Collections](collections.md) | Mapping lists, arrays, IEnumerable, HashSet |
| [Projections](projections.md) | CreateProjection, ProjectTo with EF Core |
| [Advanced](advanced.md) | Polymorphism, circular references, MaxDepth, custom resolvers |
| [Migration from AutoMapper](migration-from-automapper.md) | Step-by-step migration guide |
| [AOT Guide](aot-guide.md) | Native AOT setup and constraints |
| [Diagnostics](diagnostics.md) | Full catalog of OM-series diagnostic codes |

## Architecture

OpenAutoMapper is composed of five packages:

```
OpenAutoMapper.Abstractions (netstandard2.0; net8.0; net9.0)
    Interfaces, attributes, base types. Zero NuGet dependencies.
        |
OpenAutoMapper.Core (net8.0; net9.0)
    Profile, MapperConfiguration, fluent API implementation.
        |
OpenAutoMapper.Generator (netstandard2.0)
    Roslyn incremental source generator. Emits mapping code at compile time.
        |
OpenAutoMapper (net8.0; net9.0)
    Meta-package: bundles Core + Generator (as analyzer).
        |
OpenAutoMapper.DependencyInjection (net8.0; net9.0)
    AddAutoMapper() / AddOpenAutoMapper() extension methods.
```

Install `OpenAutoMapper` to get everything except DI. Add `OpenAutoMapper.DependencyInjection` if you use `Microsoft.Extensions.DependencyInjection`.
