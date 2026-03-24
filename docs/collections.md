---
layout: default
title: Collections
nav_order: 4
---

# Collections

OpenAutoMapper automatically maps collections when a mapping exists between the element types. You do not need to create separate mappings for collection types.

## Supported Collection Types

Given a mapping `CreateMap<Source, Dest>()`, the following collection mappings are automatically available:

| Source Type | Destination Type | Notes |
|---|---|---|
| `List<Source>` | `List<Dest>` | Most common case |
| `Source[]` | `Dest[]` | Array-to-array |
| `List<Source>` | `Dest[]` | List-to-array |
| `Source[]` | `List<Dest>` | Array-to-list |
| `IEnumerable<Source>` | `List<Dest>` | Any enumerable to list |
| `IEnumerable<Source>` | `Dest[]` | Any enumerable to array |
| `ICollection<Source>` | `ICollection<Dest>` | Interface-to-interface |
| `IReadOnlyList<Source>` | `List<Dest>` | Read-only to mutable |
| `HashSet<Source>` | `HashSet<Dest>` | Set-to-set |
| `IEnumerable<Source>` | `HashSet<Dest>` | Any enumerable to set |

## Basic Collection Mapping

```csharp
// Configuration — only the element mapping is needed
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Product, ProductDto>();
});

var mapper = config.CreateMapper();

// Map a list
var products = new List<Product>
{
    new() { Id = 1, Name = "Widget", Price = 9.99m },
    new() { Id = 2, Name = "Gadget", Price = 19.99m },
};

List<ProductDto> dtos = mapper.Map<List<ProductDto>>(products);
```

## Array Mapping

```csharp
Product[] products = GetProducts();
ProductDto[] dtos = mapper.Map<ProductDto[]>(products);
```

## IEnumerable Mapping

```csharp
IEnumerable<Product> products = GetProducts();
List<ProductDto> dtos = mapper.Map<List<ProductDto>>(products);
```

The source can be any `IEnumerable<T>`. The destination type determines the concrete collection type that is created.

## HashSet Mapping

```csharp
var tags = new HashSet<Tag>
{
    new() { Name = "dotnet" },
    new() { Name = "aot" },
};

HashSet<TagDto> tagDtos = mapper.Map<HashSet<TagDto>>(tags);
```

Duplicate detection in the destination `HashSet` uses the default equality comparer for the destination type.

## Mapping into Existing Collections

You can map into an existing collection using the two-parameter `Map` overload:

```csharp
var existingList = new List<ProductDto>();
mapper.Map(products, existingList);
```

**Behavior:** the existing collection is cleared and repopulated with the mapped items.

## Null Collections

If the source collection is `null`, the mapped result will be an empty collection (not `null`):

```csharp
List<Product>? nullList = null;
var result = mapper.Map<List<ProductDto>>(nullList);
// result is an empty List<ProductDto>, not null
```

This follows the principle of avoiding null reference exceptions in mapped output.

### AllowNullCollections

To change this behavior and preserve `null` source collections as `null` in the destination, set `AllowNullCollections = true` on your Profile:

```csharp
public class MyProfile : Profile
{
    public MyProfile()
    {
        AllowNullCollections = true;

        CreateMap<Source, Dest>();
    }
}
```

With `AllowNullCollections = true`:

```csharp
List<Product>? nullList = null;
var result = mapper.Map<List<ProductDto>>(nullList);
// result is null (not an empty list)
```

This applies to all collection types (`List<T>`, `T[]`, `HashSet<T>`) and `Dictionary<K,V>` properties within the profile.

## Nested Collections

Collections inside mapped objects are handled automatically:

```csharp
public class Order
{
    public int Id { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderDto
{
    public int Id { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
}

// Just register both element mappings
cfg.CreateMap<Order, OrderDto>();
cfg.CreateMap<OrderLine, OrderLineDto>();
```

The generator detects the `List<OrderLine>` to `List<OrderLineDto>` mapping need automatically and emits the appropriate collection mapping code.

## Performance Notes

- The source generator pre-allocates the destination collection with the known capacity when the source implements `ICollection<T>` (which provides `.Count`).
- Array mappings use `Array.Copy` semantics for primitive types where possible.
- No LINQ is used in generated collection mapping code — it is a plain `for` loop.

## Next Steps

- [Projections](projections.md) — collection projections with EF Core
- [Configuration](configuration.md) — customizing individual element mappings
