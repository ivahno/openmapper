# OpenAutoMapper

**A Native AOT-first, high-performance object mapper for .NET — inspired by AutoMapper, powered by Roslyn source generators.**

[![CI](https://img.shields.io/github/actions/workflow/status/OpenAutoMapper/OpenAutoMapper/ci.yml?branch=main&label=CI)](https://github.com/OpenAutoMapper/OpenAutoMapper/actions)
[![NuGet](https://img.shields.io/nuget/v/OpenAutoMapper?label=NuGet)](https://www.nuget.org/packages/OpenAutoMapper)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## Why OpenAutoMapper?

.NET is moving fast. Native AOT, trimming, and source generators are the new baseline for high-performance applications. But the most popular object mapper — AutoMapper — was designed in a pre-AOT world. It relies on runtime reflection, `Expression.Compile()`, and dynamic code generation, which means:

- **It breaks under `PublishAot=true`** — `Expression.Compile()` throws `PlatformNotSupportedException` in NativeAOT.
- **It has measurable startup cost** — reflection-based configuration scanning adds 30-50 ms before the first request.
- **It allocates on every map** — delegate dispatch and internal bookkeeping create GC pressure on hot paths.
- **Errors surface at runtime** — unmapped properties, misconfigured profiles, and type mismatches only fail when the mapping runs, not when you build.

OpenAutoMapper solves all four problems. The Roslyn source generator emits plain C# mapping code at compile time — the same code you would write by hand. There is nothing to compile at runtime, nothing to reflect over, and nothing that can surprise you in production. Errors become compiler diagnostics. Performance matches hand-written code. And it works everywhere .NET runs, including NativeAOT, Blazor WASM, and trimmed microservices.

On top of the technical issues, AutoMapper's shift from a permissive open-source license to a more restrictive model raised concerns for teams that depend on predictable licensing for their infrastructure libraries. OpenAutoMapper is and will remain **MIT-licensed** — no license changes, no usage restrictions, no surprises.

If you already use AutoMapper, migrating is a namespace change — the API is identical by design.

---

## Quick Start

### 1. Install the NuGet package

```bash
dotnet add package OpenAutoMapper
```

### 2. Define a mapping profile

```csharp
using OpenAutoMapper;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>();
    }
}
```

### 3. Create a configuration and mapper

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<OrderProfile>();
});

var mapper = config.CreateMapper();
```

### 4. Map objects

```csharp
var order = new Order { Id = 1, CustomerName = "Jane Doe", Total = 49.99m };
var dto = mapper.Map<OrderDto>(order);
```

No reflection. No runtime code generation. Everything is emitted at compile time by the Roslyn source generator.

---

## Key Features

- **AOT-safe by design** — fully compatible with `PublishAot=true` and `TrimMode=full`
- **Zero reflection at runtime** — all mapping code is generated at compile time as plain C#
- **Zero-allocation flat maps** — simple property-to-property maps avoid heap allocations where possible
- **Direct extension methods** — `source.MapToDto()` calls bypass interface dispatch for maximum throughput on hot paths
- **AutoMapper-compatible API** — `Profile`, `CreateMap`, `ForMember`, `IMapper`, `ProjectTo<T>()` all work as expected
- **Compile-time diagnostics** — unmapped properties, open generics, and misconfiguration are reported as compiler warnings/errors (OM-series codes)
- **EF Core projections** — `CreateProjection<S,D>()` and `ProjectTo<T>()` translate to expression trees for efficient SQL generation
- **Dependency injection** — `services.AddAutoMapper()` works as a drop-in replacement
- **Init-only & record types** — `init` properties and positional records are mapped correctly
- **Constructor mapping** — `ForCtorParam()` maps values into constructor parameters for immutable types
- **ForAllMembers** — apply a configuration action (e.g., `Ignore()`) to every destination member at once
- **IncludeMembers** — flatten child objects from the source into the destination
- **Enum strategies** — map enums by name, by value, or case-insensitive via `CreateMap<S,D>().UseEnumMappingStrategy()`
- **AllowNullCollections** — preserve `null` collection properties instead of substituting empty collections
- **Dictionary-to-object mapping** — map `Dictionary<string, object>` keys to destination properties by name
- **Deep cloning** — `UseDeepCloning()` produces fully independent copies of object graphs
- **Multiple source mapping** — `IncludeSource<T>()` merges multiple source types into a single destination
- **Named mappings** — map the same type pair with different configurations using named/tagged profiles

---

## Performance

Benchmarked on .NET 10 (BenchmarkDotNet, Apple M-series, Release mode):

| Scenario | OpenAutoMapper | Mapperly 4.3 | Mapster 10.0 | AutoMapper 16.1 | Hand-written |
|---|---|---|---|---|---|
| Flat DTO (10 props) | **14.16 ns** | 14.68 ns | 21.14 ns | 59.82 ns | 13.70 ns |
| Nested DTO | **~19 ns** | ~19 ns | ~28 ns | ~85 ns | ~18 ns |
| Collection (100 items) | **~2.1 us** | ~2.1 us | ~3.5 us | ~8.2 us | ~2.0 us |
| Startup (cold start) | **~0 ms** | ~0 ms | ~5 ms | ~30 ms | N/A |

OpenAutoMapper matches hand-written performance when called via the generated extension method (`source.MapToDestDto()`). The `IMapper` interface path adds ~5 ns of dispatch overhead — use direct calls on hot paths.

### NativeAOT Performance

Under NativeAOT (where AutoMapper and Mapster cannot run):

| Scenario | OpenAutoMapper | Mapperly | Hand-written |
|---|---|---|---|
| Flat DTO | **14.62 ns** (0.94x) | 15.10 ns (0.98x) | 15.48 ns |
| Nested DTO | 19.86 ns (1.10x) | 19.61 ns (1.08x) | 18.10 ns |

> Run benchmarks yourself: `dotnet run --project tests/OpenAutoMapper.Benchmarks/ -c Release -- --filter '*Competitive*'`

---

## EF Core Projections

OpenAutoMapper generates `Expression<Func<S,D>>` trees that EF Core translates directly to SQL — no entity materialization, no N+1 queries:

```csharp
// Profile
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateProjection<Customer, CustomerDto>();
        CreateProjection<Address, AddressDto>();
        CreateProjection<Order, OrderSummaryDto>();
        CreateProjection<OrderLine, OrderLineDto>();
    }
}

// Query — generates a single SELECT with only the DTO columns
var summaries = await db.Orders
    .Where(o => o.Total > 100m)
    .OrderByDescending(o => o.Total)
    .ProjectToOrderSummaryDto()
    .Take(20)
    .ToListAsync();
```

The generator handles nested objects, nested collections, and convention-based flattening:

```csharp
// Nested object: Customer.Address → CustomerDto.Address (AddressDto)
var customers = db.Customers.ProjectToCustomerDto().ToList();

// Nested collection: Order.Lines → OrderDetailDto.Lines (List<OrderLineDto>)
var details = db.Orders.ProjectToOrderDetailDto().ToList();

// Flattening by convention: Order.Customer.Name → OrderDto.CustomerName
var orders = db.Orders.ProjectToOrderDto().ToList();
```

See the [EF Core sample](samples/OpenAutoMapper.Samples.EfCore/) for a complete working example, and the [Projections guide](docs/projections.md) for the full API.

### Projection Limitations

The source generator emits expression trees at compile time. This means:

- **Navigation-chain `MapFrom` expressions** like `s.Customer.Name` or `s.Lines.Count` are not translated into the generated expression tree. Use convention-based flattening instead (name the DTO property `CustomerName` to auto-map from `Customer.Name`).
- **Runtime-only features** (`BeforeMap`, `AfterMap`, `Condition`, `ConstructUsing`) cannot be used in projections — they cannot be translated to SQL.
- **Dictionary properties** are not supported in projections and will emit diagnostic OM1042.

---

## What Makes This Different

AutoMapper is a mature, battle-tested library — but it relies heavily on runtime reflection and `Expression.Compile()`, which makes it incompatible with Native AOT and incurs startup and per-map overhead.

OpenAutoMapper takes a different approach:

1. **Roslyn source generator** — the mapping code is emitted as plain C# during compilation, not at runtime.
2. **No `Expression.Compile()`** — expressions are only used for EF Core `ProjectTo<T>()` projections (which are translated to SQL, not compiled).
3. **Same API surface** — if you know AutoMapper, you already know OpenAutoMapper. `Profile`, `CreateMap`, `ForMember`, `Ignore`, `ReverseMap`, `IMapper` — it is all here.
4. **Diagnostic-first** — misconfiguration is caught at compile time with clear OM-series diagnostic messages, not at runtime with cryptic exceptions.

---

## AOT Limitations

OpenAutoMapper is designed for Native AOT from the ground up. However, a few things to be aware of:

- **Open generic mappings** (`CreateMap(typeof(Wrapper<>), typeof(WrapperDto<>))`) are not supported — the generator needs concrete types.
- **Interface destination types** are not supported — the generator must know which concrete type to instantiate.
- **Dynamic/runtime map creation** is not available — all mappings must be declared at compile time.
- **Custom `ITypeConverter<S,D>` implementations** must be concrete, non-generic classes.

See the [AOT Guide](docs/aot-guide.md) for full details.

---

## Migration from AutoMapper

Migrating from AutoMapper is a three-step process:

### Step 1: Replace the NuGet package

```bash
dotnet remove package AutoMapper
dotnet remove package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package OpenAutoMapper
dotnet add package OpenAutoMapper.DependencyInjection
```

### Step 2: Update the using statements

```diff
- using AutoMapper;
+ using OpenAutoMapper;
```

### Step 3: Build and fix diagnostics

```bash
dotnet build
```

The source generator will report any unsupported patterns as OM-series diagnostics. Fix those and you are done.

See the full [Migration Guide](docs/migration-from-automapper.md) for a detailed walkthrough.

---

## Documentation

- [Getting Started](docs/getting-started.md)
- [Configuration](docs/configuration.md)
- [Collections](docs/collections.md)
- [Projections (EF Core)](docs/projections.md)
- [Advanced Topics](docs/advanced.md)
- [Migration from AutoMapper](docs/migration-from-automapper.md)
- [AOT Guide](docs/aot-guide.md)
- [Diagnostics Reference](docs/diagnostics.md)

---

## Contributing

Contributions are welcome! Please see the following guidelines:

1. Fork the repository and create a feature branch from `main`.
2. Ensure all existing tests pass: `dotnet test`
3. Add tests for any new functionality.
4. Run `dotnet build -c Release` to confirm no warnings-as-errors.
5. Submit a pull request with a clear description of the change.

### Building from Source

```bash
git clone https://github.com/OpenAutoMapper/OpenAutoMapper.git
cd OpenAutoMapper
dotnet restore
dotnet build -c Release
dotnet test
```

---

## License

This project is licensed under the [MIT License](LICENSE).

Copyright (c) 2026 D. Ivahno
