using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using Mapster;

namespace OpenAutoMapper.Benchmarks;

// ============================================================
// Flat DTO — 10 properties, single object
// ============================================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class CompetitiveFlatMapBenchmarks
{
    private OpenAutoMapper.IMapper _openAutoMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private OrderSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        // OpenAutoMapper (source-generated)
        var oamConfig = new OpenAutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _openAutoMapper = oamConfig.CreateMapper();

        // AutoMapper (reflection-based)
        var amConfig = new global::AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderSource, OrderDest>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _autoMapper = amConfig.CreateMapper();

        // Mapster (runtime code generation)
        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<OrderSource, OrderDest>();
        _mapsterConfig.Compile();

        _source = new OrderSource
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Amount = 99.99m,
            Tax = 8.50m,
            Discount = 5.00m,
            Currency = "USD",
            Notes = "Test order",
            IsActive = true
        };
    }

    [Benchmark(Baseline = true)]
    public OrderDest HandWritten()
    {
        return new OrderDest
        {
            Id = _source.Id,
            OrderNumber = _source.OrderNumber,
            CustomerName = _source.CustomerName,
            CustomerEmail = _source.CustomerEmail,
            Amount = _source.Amount,
            Tax = _source.Tax,
            Discount = _source.Discount,
            Currency = _source.Currency,
            Notes = _source.Notes,
            IsActive = _source.IsActive
        };
    }

    [Benchmark]
    public OrderDest OpenAutoMapper_Direct()
    {
        return _source.MapToOrderDest();
    }

    [Benchmark]
    public OrderDest OpenAutoMapper_IMapper()
    {
        return _openAutoMapper.Map<OrderSource, OrderDest>(_source);
    }

    [Benchmark]
    public OrderDest AutoMapper_Refl()
    {
        return _autoMapper.Map<OrderDest>(_source);
    }

    [Benchmark]
    public OrderDest Mapster_CodeGen()
    {
        return _source.Adapt<OrderDest>(_mapsterConfig);
    }

    [Benchmark]
    public OrderDest Mapperly_Gen()
    {
        return MapperlyOrderMapper.MapOrder(_source);
    }
}

// ============================================================
// Nested DTO — object with sub-object (Address)
// ============================================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class CompetitiveNestedMapBenchmarks
{
    private OpenAutoMapper.IMapper _openAutoMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private CustomerSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var oamConfig = new OpenAutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _openAutoMapper = oamConfig.CreateMapper();

        var amConfig = new global::AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CustomerSource, CustomerDest>();
            cfg.CreateMap<AddressSource, AddressDest>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _autoMapper = amConfig.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<CustomerSource, CustomerDest>();
        _mapsterConfig.NewConfig<AddressSource, AddressDest>();
        _mapsterConfig.Compile();

        _source = new CustomerSource
        {
            Id = 1,
            Name = "John Doe",
            Address = new AddressSource
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }
        };
    }

    [Benchmark(Baseline = true)]
    public CustomerDest HandWritten()
    {
        return new CustomerDest
        {
            Id = _source.Id,
            Name = _source.Name,
            Address = _source.Address is not null
                ? new AddressDest
                {
                    Street = _source.Address.Street,
                    City = _source.Address.City,
                    State = _source.Address.State,
                    Zip = _source.Address.Zip
                }
                : null
        };
    }

    [Benchmark]
    public CustomerDest OpenAutoMapper_Direct()
    {
        return _source.MapToCustomerDest();
    }

    [Benchmark]
    public CustomerDest OpenAutoMapper_IMapper()
    {
        return _openAutoMapper.Map<CustomerSource, CustomerDest>(_source);
    }

    [Benchmark]
    public CustomerDest AutoMapper_Refl()
    {
        return _autoMapper.Map<CustomerDest>(_source);
    }

    [Benchmark]
    public CustomerDest Mapster_CodeGen()
    {
        return _source.Adapt<CustomerDest>(_mapsterConfig);
    }

    [Benchmark]
    public CustomerDest Mapperly_Gen()
    {
        return MapperlyCustomerMapper.MapCustomer(_source);
    }
}

// ============================================================
// Collection — 100 items with nested mapping
// ============================================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class CompetitiveCollectionMapBenchmarks
{
    private OpenAutoMapper.IMapper _openAutoMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private OrderWithItemsSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var oamConfig = new OpenAutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _openAutoMapper = oamConfig.CreateMapper();

        var amConfig = new global::AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderWithItemsSource, OrderWithItemsDest>();
            cfg.CreateMap<LineItemSource, LineItemDest>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _autoMapper = amConfig.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<OrderWithItemsSource, OrderWithItemsDest>();
        _mapsterConfig.NewConfig<LineItemSource, LineItemDest>();
        _mapsterConfig.Compile();

        _source = new OrderWithItemsSource
        {
            Id = 1,
            Items = Enumerable.Range(1, 100).Select(i => new LineItemSource
            {
                ProductName = $"Product {i}",
                Quantity = i,
                UnitPrice = i * 10.5m
            }).ToList()
        };
    }

    [Benchmark(Baseline = true)]
    public OrderWithItemsDest HandWritten()
    {
        return new OrderWithItemsDest
        {
            Id = _source.Id,
            Items = _source.Items?.Select(i => new LineItemDest
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList() ?? new()
        };
    }

    [Benchmark]
    public OrderWithItemsDest OpenAutoMapper_Direct()
    {
        return _source.MapToOrderWithItemsDest();
    }

    [Benchmark]
    public OrderWithItemsDest OpenAutoMapper_IMapper()
    {
        return _openAutoMapper.Map<OrderWithItemsSource, OrderWithItemsDest>(_source);
    }

    [Benchmark]
    public OrderWithItemsDest AutoMapper_Refl()
    {
        return _autoMapper.Map<OrderWithItemsDest>(_source);
    }

    [Benchmark]
    public OrderWithItemsDest Mapster_CodeGen()
    {
        return _source.Adapt<OrderWithItemsDest>(_mapsterConfig);
    }

    [Benchmark]
    public OrderWithItemsDest Mapperly_Gen()
    {
        return MapperlyCollectionMapper.MapOrder(_source);
    }
}

// ============================================================
// Startup — measures configuration + first map (cold start)
// ============================================================

[MemoryDiagnoser]
[SimpleJob(iterationCount: 5, warmupCount: 1)]
[RankColumn]
public class CompetitiveStartupBenchmarks
{
    private OrderSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = new OrderSource
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Amount = 99.99m,
            Tax = 8.50m,
            Discount = 5.00m,
            Currency = "USD",
            Notes = "Test order",
            IsActive = true
        };
    }

    [Benchmark(Baseline = true)]
    public OrderDest OpenAutoMapper_Startup()
    {
        var config = new OpenAutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        var mapper = config.CreateMapper();
        return mapper.Map<OrderSource, OrderDest>(_source);
    }

    [Benchmark]
    public OrderDest AutoMapper_Startup()
    {
        var config = new global::AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderSource, OrderDest>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();
        return mapper.Map<OrderDest>(_source);
    }

    [Benchmark]
    public OrderDest Mapster_Startup()
    {
        var mapsterConfig = new TypeAdapterConfig();
        mapsterConfig.NewConfig<OrderSource, OrderDest>();
        mapsterConfig.Compile();
        return _source.Adapt<OrderDest>(mapsterConfig);
    }

    [Benchmark]
    public OrderDest Mapperly_Startup()
    {
        // Mapperly is source-generated — no configuration needed
        return MapperlyOrderMapper.MapOrder(_source);
    }
}
