using BenchmarkDotNet.Attributes;
using OpenAutoMapper;

namespace OpenAutoMapper.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class FlatMappingBenchmarks
{
    private IMapper _mapper = null!;
    private OrderSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _mapper = config.CreateMapper();
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
    public OrderDest OpenAutoMapper_FlatDto()
    {
        return _mapper.Map<OrderSource, OrderDest>(_source);
    }

    [Benchmark]
    public OrderDest HandWritten_FlatDto()
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
}

[MemoryDiagnoser]
[SimpleJob]
public class NestedMappingBenchmarks
{
    private IMapper _mapper = null!;
    private CustomerSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _mapper = config.CreateMapper();
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
    public CustomerDest OpenAutoMapper_NestedDto()
    {
        return _mapper.Map<CustomerSource, CustomerDest>(_source);
    }

    [Benchmark]
    public CustomerDest HandWritten_NestedDto()
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
}

[MemoryDiagnoser]
[SimpleJob]
public class CollectionMappingBenchmarks
{
    private IMapper _mapper = null!;
    private OrderWithItemsSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BenchmarkProfile>();
        });
        _mapper = config.CreateMapper();
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
    public OrderWithItemsDest OpenAutoMapper_Collection100()
    {
        return _mapper.Map<OrderWithItemsSource, OrderWithItemsDest>(_source);
    }

    [Benchmark]
    public OrderWithItemsDest HandWritten_Collection100()
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
}

// --- Models ---

public class OrderSource
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public string Currency { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsActive { get; set; }
}

public class OrderDest
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public string Currency { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsActive { get; set; }
}

public class CustomerSource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AddressSource? Address { get; set; }
}

public class CustomerDest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AddressDest? Address { get; set; }
}

public class AddressSource
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class AddressDest
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class OrderWithItemsSource
{
    public int Id { get; set; }
    public List<LineItemSource>? Items { get; set; }
}

public class OrderWithItemsDest
{
    public int Id { get; set; }
    public List<LineItemDest>? Items { get; set; }
}

public class LineItemSource
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class LineItemDest
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class BenchmarkProfile : Profile
{
    public BenchmarkProfile()
    {
        CreateMap<OrderSource, OrderDest>();
        CreateMap<CustomerSource, CustomerDest>();
        CreateMap<AddressSource, AddressDest>();
        CreateMap<OrderWithItemsSource, OrderWithItemsDest>();
        CreateMap<LineItemSource, LineItemDest>();
    }
}
