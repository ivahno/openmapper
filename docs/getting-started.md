---
layout: default
title: Getting Started
nav_order: 2
---

# Getting Started

This guide walks you through installing OpenAutoMapper, creating your first mapping, organizing mappings with Profiles, and registering with dependency injection.

## Installation

### Basic usage (console apps, libraries)

```bash
dotnet add package OpenAutoMapper
```

### With dependency injection (ASP.NET Core, Worker Services)

```bash
dotnet add package OpenAutoMapper
dotnet add package OpenAutoMapper.DependencyInjection
```

## Your First Mapping

### 1. Define source and destination types

```csharp
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### 2. Create a configuration and map

```csharp
using OpenAutoMapper;

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
});

var mapper = config.CreateMapper();

var customer = new Customer { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };
var dto = mapper.Map<CustomerDto>(customer);

Console.WriteLine($"{dto.FirstName} {dto.LastName}"); // Jane Doe
```

Properties are matched by name. If the source and destination have properties with the same name and compatible types, they are mapped automatically.

## Using Profiles

Profiles let you organize mapping configurations into logical groups. Create a class that inherits from `Profile` and call `CreateMap` in the constructor:

```csharp
using OpenAutoMapper;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerSummary>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName));
    }
}
```

Then register the profile in your configuration:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<CustomerProfile>();
});
```

Or register all profiles from an assembly:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddMaps(typeof(CustomerProfile).Assembly);
});
```

## Dependency Injection

### ASP.NET Core / Generic Host

Add the `OpenAutoMapper.DependencyInjection` package and call `AddAutoMapper()` in your `Program.cs`:

```csharp
using OpenAutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Scans the assembly containing CustomerProfile for all Profile subclasses
builder.Services.AddAutoMapper(typeof(CustomerProfile).Assembly);

var app = builder.Build();
```

This registers:
- `MapperConfiguration` as a singleton
- `IConfigurationProvider` as a singleton
- `IMapper` as a singleton

### Injecting the mapper

```csharp
public class CustomerService
{
    private readonly IMapper _mapper;

    public CustomerService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public CustomerDto GetCustomer(Customer customer)
    {
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

### Alternative: AddOpenAutoMapper

If you prefer an explicitly named method (to avoid confusion with the original AutoMapper), you can use:

```csharp
builder.Services.AddOpenAutoMapper(typeof(CustomerProfile).Assembly);
```

Both methods are functionally identical.

## Validating Configuration

Call `AssertConfigurationIsValid()` to verify that all destination properties have a corresponding source property or explicit configuration:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<CustomerProfile>();
});

config.AssertConfigurationIsValid(); // Throws if unmapped properties exist
```

This is especially useful in integration tests:

```csharp
[Fact]
public void Configuration_IsValid()
{
    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<CustomerProfile>();
    });

    config.AssertConfigurationIsValid();
}
```

## Performance: Direct Extension Methods

The source generator creates public extension methods like `MapToCustomerDto()` for each mapping. When types are known at compile time, calling these directly bypasses the `IMapper` interface dispatch for maximum performance:

```csharp
// Standard — goes through IMapper interface dispatch (~19 ns)
var dto = mapper.Map<CustomerDto>(customer);

// Direct — calls the generated extension method (~14 ns, matches hand-written)
var dto = customer.MapToCustomerDto();
```

Both produce identical results. Use `IMapper` when you need the AutoMapper-compatible API, dependency injection, or runtime type dispatch. Use the direct extension method on hot paths where every nanosecond matters.

The extension methods are available in the same namespace as the destination type. No additional imports are needed if you already reference the destination type's namespace.

## Next Steps

- [Configuration](configuration.md) — learn about ForMember, Ignore, Condition, and more
- [Collections](collections.md) — map lists, arrays, and other collections
- [Projections](projections.md) — use ProjectTo with EF Core
