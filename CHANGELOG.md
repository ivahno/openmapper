# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2026-09-26

### Security

- Update Microsoft.SourceLink.GitHub to 10.0.401, which pulls Microsoft.Build.Tasks.Git 10.0.401 and clears GHSA-23fw-v26w-5fgq.
- Update Microsoft.EntityFrameworkCore.Sqlite to 10.0.12, which pulls SQLitePCLRaw 2.1.12 (SQLite 3.53.3) and clears GHSA-2m69-gcr7-jv3q.

### Changed

- Update the remaining direct NuGet packages to current stable versions, including Microsoft.Extensions 10.0.12, Roslyn 4.14.0, the test SDK 18.10.1, FluentAssertions 8.11.0, xunit.runner.visualstudio 4.0.0, AutoMapper 16.2.0, and Mapster 10.0.13.
- Update GitHub Actions `actions/checkout` to v7 and `actions/setup-dotnet` to v6.

## [Unreleased]

### Added

- Initial project structure and build infrastructure
- OpenAutoMapper.Abstractions: public API interfaces and types
- OpenAutoMapper.Core: Profile, MapperConfiguration, fluent API
- OpenAutoMapper.Generator: Roslyn incremental source generator
- OpenAutoMapper.DependencyInjection: DI integration
- CI/CD pipeline with GitHub Actions
- Comprehensive documentation
- `ForPath()` deep property path mapping with `??= new` intermediate initialization
- `RecognizePrefixes()` / `RecognizePostfixes()` for flexible property name matching
- Unflattening support (reverse of flattening, e.g., source `AddressCity` → dest `Address.City`)
- `IMemberValueResolver<TSource, TDest, TSourceMember, TDestMember>` wiring
- ForPath support in projection expressions (nested member initializers)
- .NET 10 target framework support (net10.0)
- Public generated extension methods (`source.MapToDest()`) for zero-dispatch performance
- Competitive benchmarks against AutoMapper 16.1, Mapster 10.0, and Mapperly 4.3
- NativeAOT benchmark app proving performance parity with hand-written code under AOT
- CI coverage gate (70% minimum line coverage)
- CI performance regression gate (150% threshold via benchmark-action)
- Init-only property and record type support in generated mappings
- Constructor mapping via `ForCtorParam()` for immutable types
- `ForAllMembers()` for applying a configuration action to every destination member
- `IncludeMembers()` for flattening multiple source child objects into a single destination
- Enum mapping strategies: `ByName`, `ByValue`, and `CaseInsensitive` (configurable per-map or globally)
- `AllowNullCollections` option (global and per-map) to preserve null instead of emitting empty collections
- Dictionary-to-object mapping (source `Dictionary<string, object>` keys mapped to destination properties)
- Deep cloning via `UseDeepCloning()` for creating fully independent object graphs
- Multiple source mapping via `IncludeSource<T>()` for merging several source types into one destination
- Named/tagged mappings for mapping the same type pair with different configurations
