using OpenAutoMapper;
using Riok.Mapperly.Abstractions;

namespace OpenAutoMapper.Benchmarks.Aot;

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

public class AotBenchmarkProfile : Profile
{
    public AotBenchmarkProfile()
    {
        CreateMap<OrderSource, OrderDest>();
        CreateMap<CustomerSource, CustomerDest>();
        CreateMap<AddressSource, AddressDest>();
    }
}

[Mapper]
public static partial class AotMapperlyMapper
{
    public static partial OrderDest MapOrder(OrderSource source);
    public static partial CustomerDest MapCustomer(CustomerSource source);
    private static partial AddressDest MapAddress(AddressSource source);
}
