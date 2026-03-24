---
layout: default
title: Diagnostics
nav_order: 9
---

# Diagnostics Reference

OpenAutoMapper uses Roslyn analyzer diagnostics to report mapping issues at compile time. All diagnostics use the `OM` prefix followed by a four-digit code.

## Diagnostic Severity Levels

| Severity | Meaning |
|---|---|
| **Error** | The mapping cannot be generated. The build will fail. |
| **Warning** | The mapping will be generated, but there may be a problem. |
| **Info** | Informational message about the mapping. |

## OM1001: Source type unknown at compile time

**Severity:** Error

**Description:** The source type in a `CreateMap` call could not be resolved at compile time. This typically happens when using `object`, `dynamic`, or an unresolved generic type parameter as the source.

**Example (triggers diagnostic):**

```csharp
// OM1001: Cannot determine source type at compile time
cfg.CreateMap<object, Dest>();
```

**Fix:** Use a concrete source type:

```csharp
cfg.CreateMap<Source, Dest>();
```

---

## OM1002: Destination type unknown at compile time

**Severity:** Error

**Description:** The destination type in a `CreateMap` call could not be resolved at compile time.

**Example (triggers diagnostic):**

```csharp
// OM1002: Cannot determine destination type at compile time
cfg.CreateMap<Source, object>();
```

**Fix:** Use a concrete destination type:

```csharp
cfg.CreateMap<Source, Dest>();
```

---

## OM1003: Open generic mapping not supported

**Severity:** Error

**Description:** OpenAutoMapper does not support open generic mappings because the source generator requires concrete type information to emit mapping code.

**Example (triggers diagnostic):**

```csharp
// OM1003: Open generic mappings are not supported
cfg.CreateMap(typeof(Wrapper<>), typeof(WrapperDto<>));
```

**Fix:** Create explicit mappings for each concrete generic type:

```csharp
cfg.CreateMap<Wrapper<int>, WrapperDto<int>>();
cfg.CreateMap<Wrapper<string>, WrapperDto<string>>();
cfg.CreateMap<Wrapper<Order>, WrapperDto<OrderDto>>();
```

---

## OM1004: Interface destination type not supported

**Severity:** Error

**Description:** The destination type is an interface. The source generator cannot instantiate an interface — it needs a concrete type to emit `new TDestination()`.

**Example (triggers diagnostic):**

```csharp
// OM1004: Cannot map to interface type 'IOrderDto'
cfg.CreateMap<Order, IOrderDto>();
```

**Fix:** Map to a concrete class that implements the interface:

```csharp
cfg.CreateMap<Order, OrderDto>();  // OrderDto : IOrderDto
```

---

## OM1010: Unmapped destination property

**Severity:** Warning

**Description:** A property on the destination type has no corresponding source property (by name) and no explicit configuration (`ForMember`, `MapFrom`, `Ignore`).

**Example (triggers diagnostic):**

```csharp
public class Source
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Dest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }  // OM1010: No matching source property
}

cfg.CreateMap<Source, Dest>();
```

**Fix (option 1):** Add a `ForMember` configuration:

```csharp
cfg.CreateMap<Source, Dest>()
    .ForMember(d => d.DisplayName, opt => opt.MapFrom(s => s.Name));
```

**Fix (option 2):** Ignore the property:

```csharp
cfg.CreateMap<Source, Dest>()
    .Ignore(d => d.DisplayName);
```

**Fix (option 3):** Use `MemberList.None` to skip validation:

```csharp
cfg.CreateMap<Source, Dest>(MemberList.None);
```

---

## OM1011: Sensitive property mapped without explicit configuration

**Severity:** Warning

**Description:** A source property marked with `[SensitiveProperty]` is being mapped to the destination without explicit `ForMember` configuration. This is a safety check to prevent accidental exposure of sensitive data (e.g., passwords, SSNs, tokens).

**Example (triggers diagnostic):**

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }

    [SensitiveProperty]
    public string PasswordHash { get; set; }  // Sensitive!
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }  // OM1011: Mapped without explicit config
}

cfg.CreateMap<User, UserDto>();
```

**Fix (option 1):** Explicitly ignore the property:

```csharp
cfg.CreateMap<User, UserDto>()
    .Ignore(d => d.PasswordHash);
```

**Fix (option 2):** Explicitly map the property (acknowledging the sensitivity):

```csharp
cfg.CreateMap<User, UserDto>()
    .ForMember(d => d.PasswordHash, opt => opt.MapFrom(s => s.PasswordHash));
```

**Fix (option 3):** Remove the property from the destination type entirely.

---

---

## OM1020: Invalid MaxDepth value

**Severity:** Error

**Description:** The `MaxDepth` value must be a positive integer.

---

## OM1021: Include type mismatch

**Severity:** Error

**Description:** The type specified in `Include<TDerived, TDerivedDest>()` does not inherit from the base source type.

---

## OM1022: IncludeBase type mismatch

**Severity:** Error

**Description:** No matching base mapping was found for `IncludeBase<TBase, TBaseDest>()`.

---

## OM1030: Circular reference detected

**Severity:** Info

**Description:** A circular reference was detected between mapped types. Depth tracking code will be emitted using the configured `MaxDepth` (default 10).

---

## OM1050: No matching constructor

**Severity:** Error

**Description:** The destination type has no parameterless constructor and no constructor whose parameters all match source properties by name. This blocks code generation for the type pair.

**Fix (option 1):** Add a parameterless constructor to the destination type.

**Fix (option 2):** Use `ForCtorParam()` to explicitly map constructor parameters:

```csharp
CreateMap<Source, Dest>()
    .ForCtorParam("id", opt => opt.MapFrom(s => s.Id));
```

---

## OM1051: Constructor parameter not found

**Severity:** Warning

**Description:** A `ForCtorParam("name")` configuration specifies a parameter name that does not exist on any public constructor of the destination type.

**Fix:** Check the constructor parameter name matches exactly (case-insensitive).

---

## Summary Table

| Code | Severity | Description |
|---|---|---|
| OM1001 | Error | Source type unknown at compile time |
| OM1002 | Error | Destination type unknown at compile time |
| OM1003 | Error | Open generic mapping not supported |
| OM1004 | Error | Interface destination type not supported |
| OM1010 | Warning | Unmapped destination property |
| OM1011 | Warning | Sensitive property mapped without explicit configuration |
| OM1020 | Error | Invalid MaxDepth value |
| OM1021 | Error | Include type mismatch |
| OM1022 | Error | IncludeBase type mismatch |
| OM1030 | Info | Circular reference detected |
| OM1050 | Error | No matching constructor |
| OM1051 | Warning | ForCtorParam parameter not found |

## Suppressing Diagnostics

If you need to suppress a diagnostic (not recommended in most cases), you can use the standard C# `#pragma` directive:

```csharp
#pragma warning disable OM1010
cfg.CreateMap<Source, Dest>();
#pragma warning restore OM1010
```

Or suppress in the project file:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);OM1010</NoWarn>
</PropertyGroup>
```

## Next Steps

- [Configuration](configuration.md) — how to fix common mapping issues
- [AOT Guide](aot-guide.md) — understanding AOT-related constraints
- [Migration from AutoMapper](migration-from-automapper.md) — migration guide
