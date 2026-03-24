# OpenAutoMapper — Claude Code Guide

## Project Overview

OpenAutoMapper is a Native AOT-first object mapper for .NET, using Roslyn source generators to generate all mapping code at compile time. It is designed as a near drop-in replacement for AutoMapper with identical API surface (`Profile`, `CreateMap<S,D>()`, `ForMember()`, `IMapper`, `ProjectTo<T>()`).

## Build Commands

```bash
dotnet restore                     # Restore all packages
dotnet build -c Release            # Build with warnings-as-errors
dotnet test                        # Run all tests
dotnet pack -c Release -o nupkg/   # Create NuGet packages
```

## Architecture

### Dependency Graph

```
OpenAutoMapper.Abstractions (netstandard2.0;net8.0;net9.0;net10.0) — zero NuGet deps
    ↑
OpenAutoMapper.Core (net8.0;net9.0;net10.0) — refs Abstractions only
    ↑
OpenAutoMapper.Generator (netstandard2.0) — refs CodeAnalysis.CSharp
    ↑
OpenAutoMapper (net8.0;net9.0;net10.0) — meta-pkg: Core + Generator-as-analyzer
    ↑
OpenAutoMapper.DependencyInjection (net8.0;net9.0;net10.0) — refs OpenAutoMapper + MS.Ext.DI.Abstractions
```

### Key Namespaces

- `OpenAutoMapper` — public API (IMapper, Profile, MapperConfiguration, etc.)
- `OpenAutoMapper.Internal` — internal configuration models (TypeMapConfiguration, PropertyMap, etc.)
- `OpenAutoMapper.Exceptions` — exception types
- `OpenAutoMapper.Attributes` — mapping attributes ([AutoMap], [IgnoreMap], etc.)

## Critical Constraints

1. **No runtime reflection** — all mapping is generated at compile time as plain C#
2. **No Expression.Compile()** — this throws under Native AOT
3. **Generator targets netstandard2.0** — must set `ImplicitUsings=disable` and cannot use modern C# APIs
4. **Abstractions targets netstandard2.0** — must set `ImplicitUsings=disable`, `IsAotCompatible` is conditional
5. **Central Package Management** — all `<PackageReference>` items omit `Version`; versions only in `Directory.Packages.props`
6. **Generator as Analyzer** — In OpenAutoMapper.csproj: `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`. In test project: `OutputItemType="None" ReferenceOutputAssembly="true"`

## Diagnostic Codes

- OM1001: Source type unknown at compile time
- OM1002: Destination type unknown at compile time
- OM1003: Open generic mapping not supported
- OM1004: Interface target not supported
- OM1010: Unmapped destination property
- OM1011: Sensitive property mapped without explicit configuration
- OM1020: Invalid MaxDepth value
- OM1021: Include type mismatch
- OM1022: IncludeBase type mismatch
- OM1030: Circular reference detected (info)
- OM1040: Circular reference in projection
- OM1041: Polymorphic Include in projection
- OM1042: Dictionary property in projection (warning)
- OM1050: No matching constructor for destination type
- OM1051: ForCtorParam parameter not found on any constructor (warning)

## Test Strategy

- **Abstractions.Tests** — enum values, attribute construction, basic types
- **Core.Tests** — Profile registration, MapperConfiguration, exceptions
- **Generator.Tests** — Verify-based snapshot tests + diagnostic assertions
- **Integration.Tests** — End-to-end: create profile, build config, map objects (generator runs as analyzer)

## Coding Conventions

- File-scoped namespaces
- `_camelCase` for private fields
- `sealed` on classes where inheritance is not intended
- All public API matches AutoMapper signatures exactly
