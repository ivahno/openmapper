#!/usr/bin/env bash
#
# Packaged-consumer smoke test.
#
# Packs the real NuGet packages, installs them into a throwaway consumer project,
# and asserts that the source generator is actually DELIVERED and RUNS — i.e. that
# `dotnet add package OpenAutoMapper` produces working mapping code, and that
# resolving IMapper through the DI package works.
#
# This is the test that would have caught the 1.0.0 packaging bug (generator never
# shipped to consumers) and the DI factory bug (MapperFactoryWithServiceCtor never
# registered, so IMapper resolution threw). The in-repo test projects do NOT wire
# the generator as an analyzer the way a real consumer does, so they cannot catch
# these regressions.
#
# Usage: eng/package-smoke-test.sh
# Requires: dotnet SDK on PATH. Network access to nuget.org (for the BCL packages).

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WORK="$(mktemp -d)"
FEED="$WORK/feed"
CONSUMER="$WORK/consumer"
CONFIG="${CONFIGURATION:-Release}"
trap 'rm -rf "$WORK"' EXIT

echo "==> Building solution, then packing to local feed: $FEED"
mkdir -p "$FEED"
# Build once, then pack with --no-build (exactly what release.yml does). This is
# deterministic: with --no-build there are no parallel per-TFM inner builds during
# pack, so pack's collection of the static None analyzer item cannot race the
# generator's build. (Plain `dotnet pack` rebuilds during pack and intermittently
# drops the analyzer when the multi-TFM inner builds race the file collection.)
if ! dotnet build "$REPO_ROOT/OpenAutoMapper.slnx" -c "$CONFIG" >/dev/null; then
  echo "FAIL: solution build failed"
  exit 1
fi
for proj in \
  OpenAutoMapper.Abstractions \
  OpenAutoMapper.Core \
  OpenAutoMapper \
  OpenAutoMapper.DependencyInjection; do
  if ! dotnet pack "$REPO_ROOT/src/$proj/$proj.csproj" -c "$CONFIG" --no-build -o "$FEED" >/dev/null; then
    echo "FAIL: dotnet pack --no-build failed for $proj"
    exit 1
  fi
done

PKG_VERSION="$(ls "$FEED"/OpenAutoMapper.[0-9]*.nupkg 2>/dev/null | head -1 | sed -E 's/.*OpenAutoMapper\.([0-9][^/]*)\.nupkg/\1/')"
META_PKG="$FEED/OpenAutoMapper.$PKG_VERSION.nupkg"
if [ -z "$PKG_VERSION" ] || [ ! -f "$META_PKG" ]; then
  echo "FAIL: OpenAutoMapper package was not produced in $FEED"
  ls -la "$FEED" || true
  exit 1
fi
echo "==> Packed version: $PKG_VERSION"

# Assert the analyzer DLL is physically present in the meta-package.
echo "==> Verifying analyzer is bundled in the OpenAutoMapper package"
if ! unzip -l "$META_PKG" | grep -q "analyzers/dotnet/cs/OpenAutoMapper.Generator.dll"; then
  echo "FAIL: $META_PKG exists but does not contain analyzers/dotnet/cs/OpenAutoMapper.Generator.dll"
  exit 1
fi

echo "==> Creating consumer project: $CONSUMER"
mkdir -p "$CONSUMER"
cat > "$CONSUMER/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$FEED" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

cat > "$CONSUMER/consumer.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- Single install must deliver the generator (1.0.0 packaging bug). -->
    <PackageReference Include="OpenAutoMapper" Version="$PKG_VERSION" />
    <!-- DI-only install must also deliver the generator transitively. -->
    <PackageReference Include="OpenAutoMapper.DependencyInjection" Version="$PKG_VERSION" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
  </ItemGroup>
</Project>
EOF

cat > "$CONSUMER/Program.cs" <<'EOF'
using OpenAutoMapper;
using Microsoft.Extensions.DependencyInjection;

public class Order { public int Id { get; set; } public string CustomerName { get; set; } = ""; public decimal Total { get; set; } }
public class OrderDto { public int Id { get; set; } public string CustomerName { get; set; } = ""; public decimal Total { get; set; } }
public class OrderProfile : Profile { public OrderProfile() => CreateMap<Order, OrderDto>(); }

public static class Program
{
    public static int Main()
    {
        // 1) Direct MapperConfiguration flow (README quick start).
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        var mapper = config.CreateMapper();
        var dto = mapper.Map<OrderDto>(new Order { Id = 1, CustomerName = "Jane Doe", Total = 49.99m });
        if (dto.Id != 1 || dto.CustomerName != "Jane Doe" || dto.Total != 49.99m)
        {
            System.Console.Error.WriteLine("FAIL: direct mapping produced wrong result");
            return 1;
        }

        // 2) DI flow — resolving IMapper exercises CreateMapper(serviceCtor).
        var sp = new ServiceCollection()
            .AddAutoMapper(cfg => cfg.CreateMap<Order, OrderDto>(), typeof(Program).Assembly)
            .BuildServiceProvider();
        var diMapper = sp.GetRequiredService<IMapper>();
        var diDto = diMapper.Map<OrderDto>(new Order { Id = 2, CustomerName = "DI", Total = 10m });
        if (diDto.Id != 2 || diDto.CustomerName != "DI")
        {
            System.Console.Error.WriteLine("FAIL: DI mapping produced wrong result");
            return 1;
        }

        System.Console.WriteLine("SMOKE OK: direct + DI mapping work from packaged generator");
        return 0;
    }
}
EOF

echo "==> Building and running consumer (generator must run as a real analyzer)"
cd "$CONSUMER"
if ! dotnet run -c "$CONFIG" 2>&1 | tee "$WORK/run.log"; then
  echo "FAIL: consumer run failed — generator likely not delivered"
  exit 1
fi

if ! grep -q "SMOKE OK" "$WORK/run.log"; then
  echo "FAIL: consumer did not print success marker"
  exit 1
fi

echo "==> PASS: packaged-consumer smoke test succeeded"
