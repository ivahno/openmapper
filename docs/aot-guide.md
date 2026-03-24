---
layout: default
title: AOT Guide
nav_order: 8
---

# Native AOT Guide

OpenAutoMapper is designed from the ground up for Native AOT compatibility. This guide covers how to set up your project for AOT publishing, what works, and what to watch out for.

## What is Native AOT?

Native AOT (Ahead-of-Time) compilation produces a self-contained native executable. The .NET runtime, JIT compiler, and all managed code are compiled directly to native machine code at publish time. This results in:

- **Faster startup** — no JIT compilation at runtime
- **Smaller footprint** — unused code is trimmed away
- **No runtime code generation** — `Reflection.Emit`, `Expression.Compile()`, and dynamic assembly loading are not available

Traditional object mappers like AutoMapper rely on `Expression.Compile()` to generate mapping delegates at runtime, which fails under AOT.

## Why OpenAutoMapper Works with AOT

OpenAutoMapper uses a Roslyn incremental source generator to emit all mapping code as plain C# at compile time. The generated code contains no reflection, no `Expression.Compile()`, and no dynamic type loading. It is equivalent to hand-written mapping code.

## Project Setup

### Minimal AOT-compatible project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <TrimMode>full</TrimMode>
    <IlcTreatWarningsAsErrors>true</IlcTreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenAutoMapper" />
  </ItemGroup>
</Project>
```

> .NET 10 is recommended for AOT. Its improved type preinitializer and JIT handle more opcodes, and benchmarks show OpenAutoMapper matching or exceeding hand-written performance under NativeAOT.

### Publishing

```bash
dotnet publish -c Release
```

The output is a single native executable with no .NET runtime dependency.

### Verify AOT compatibility

During publish, the IL compiler reports warnings for any code that is not AOT-safe. With `IlcTreatWarningsAsErrors=true`, these become hard errors. OpenAutoMapper is designed to produce zero AOT warnings.

## What Works Under AOT

| Feature | AOT Status |
|---|---|
| `CreateMap<S,D>()` | Fully supported |
| `ForMember()` / `MapFrom()` | Fully supported |
| `ForPath()` | Fully supported |
| `Ignore()` | Fully supported |
| `Condition()` / `PreCondition()` | Fully supported |
| `NullSubstitute()` | Fully supported |
| `ConstructUsing()` | Fully supported |
| `BeforeMap()` / `AfterMap()` | Fully supported |
| `ReverseMap()` | Fully supported |
| `MaxDepth()` | Fully supported |
| `Include<S,D>()` (polymorphism) | Fully supported |
| `ConvertUsing()` | Fully supported |
| Collection mapping (List, Array, etc.) | Fully supported |
| `[AutoMap]` attribute | Fully supported |
| `[IgnoreMap]` attribute | Fully supported |
| `[MapFrom]` attribute | Fully supported |
| DI with `AddAutoMapper()` | Fully supported |
| `AssertConfigurationIsValid()` | Fully supported |
| Init-only properties / records | Fully supported |
| `ForCtorParam()` | Fully supported |
| `ForAllMembers()` | Fully supported |
| `IncludeMembers()` | Fully supported |
| Enum strategies (ByName, ByValue, CaseInsensitive) | Fully supported |
| `AllowNullCollections` | Fully supported |
| `UseDeepCloning()` | Fully supported |
| `IncludeSource<T>()` (multiple sources) | Fully supported |
| Named mappings (`CreateMap<S,D>("name")`) | Fully supported |

## What Does Not Work Under AOT

| Feature | Reason | Alternative |
|---|---|---|
| Open generic mappings | AOT requires concrete types at compile time | Create explicit mappings for each type pair |
| `Map(object source, Type srcType, Type destType)` | Runtime type resolution is not AOT-safe | Use generic `Map<S,D>()` |
| `CreateProjection` + `ProjectTo` | Expression trees require EF Core AOT support | Wait for EF Core AOT improvements; use in-memory mapping instead |
| Dynamic profile loading from assemblies | Assembly scanning uses reflection | Register profiles explicitly with `AddProfile<T>()` |
| Custom `INamingConvention` | Runtime string manipulation on type metadata | Use `RecognizePrefixes` / `RecognizePostfixes` |

## AOT Performance

Under NativeAOT, OpenAutoMapper's generated code runs at or below hand-written speed. AutoMapper and Mapster cannot run under AOT at all.

| Scenario | OpenAutoMapper | Mapperly | Hand-written | AutoMapper | Mapster |
|---|---|---|---|---|---|
| Flat DTO (10 props) | **14.85 ns** | 14.88 ns | 14.02 ns | N/A | N/A |
| Nested DTO | **19.76 ns** | 19.99 ns | 18.11 ns | N/A | N/A |

For maximum AOT throughput, use the generated extension methods directly:

```csharp
// Best performance under AOT — direct static call, no dispatch
var dto = source.MapToOrderDto();
```

Run the AOT benchmark yourself:

```bash
dotnet publish tests/OpenAutoMapper.Benchmarks.Aot/ -c Release -r linux-x64
./tests/OpenAutoMapper.Benchmarks.Aot/bin/Release/net10.0/linux-x64/publish/OpenAutoMapper.Benchmarks.Aot
```

## AOT-Specific Best Practices

### 1. Use direct extension methods on hot paths

```csharp
// Fastest — direct generated extension method
var dto = source.MapToOrderDto();

// Also fast — generic Map, AOT-safe
var dto = mapper.Map<Source, Dest>(source);

// Avoid if possible — requires runtime type info
var dto = mapper.Map(source, typeof(Source), typeof(Dest));
```

### 2. Register profiles explicitly

```csharp
// Preferred for AOT
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<OrderProfile>();
    cfg.AddProfile<CustomerProfile>();
});

// Avoid for AOT — assembly scanning uses reflection
var config = new MapperConfiguration(cfg =>
{
    cfg.AddMaps(typeof(OrderProfile).Assembly);
});
```

### 3. Avoid open generics

```csharp
// NOT supported under AOT
CreateMap(typeof(Wrapper<>), typeof(WrapperDto<>));

// DO this instead
CreateMap<Wrapper<int>, WrapperDto<int>>();
CreateMap<Wrapper<string>, WrapperDto<string>>();
```

### 4. Use concrete types for value resolvers

```csharp
// Supported
public class FullNameResolver : IValueResolver<Customer, CustomerDto, string> { ... }

// NOT supported — open generic resolver
public class GenericResolver<T> : IValueResolver<T, T, string> { ... }
```

### 5. Test with PublishAot during CI

Add an AOT publish step to your CI pipeline to catch regressions early:

```yaml
- name: Verify AOT compatibility
  run: dotnet publish -c Release -r linux-x64
```

## Troubleshooting

### "No mapper factory registered" at runtime

This means the source generator did not emit a mapper implementation. Common causes:

1. The `OpenAutoMapper.Generator` package is not referenced (or not referenced as an analyzer).
2. There are no `CreateMap` calls or `[AutoMap]` attributes in the project.
3. A diagnostic error (OM1001-OM1004) prevented code generation.

**Fix:** Check the build output for OM-series diagnostics and resolve them.

### ILC warnings about reflection

If you see ILC warnings mentioning `System.Reflection` or `MakeGenericType`, check that:

1. You are not using `AddMaps(assembly)` — switch to explicit `AddProfile<T>()`.
2. You are not using `Map(object, Type, Type)` — switch to generic overloads.
3. Your custom resolvers/converters are concrete, non-generic classes.

### Trimming removes a mapping

If a mapping works in Debug but not in a trimmed Release build:

1. Ensure the source and destination types are referenced somewhere in code (not just in configuration).
2. Add `[DynamicallyAccessedMembers]` if needed (the generator should handle this automatically).

## Next Steps

- [Diagnostics](diagnostics.md) — full catalog of OM-series diagnostic codes
- [Migration from AutoMapper](migration-from-automapper.md) — migration guide
- [Getting Started](getting-started.md) — basic setup
