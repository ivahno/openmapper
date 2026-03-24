using Microsoft.EntityFrameworkCore;
using OpenAutoMapper;

namespace OpenAutoMapper.Samples.EfCore;

// ---- Entities ----

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public Address? Address { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Address
{
    public int Id { get; set; }
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
    public int CustomerId { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int OrderId { get; set; }
}

// ---- DTOs ----

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AddressDto? Address { get; set; }
}

public class AddressDto
{
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";
    public int LineCount { get; set; }
}

public class OrderLineDto
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDetailDto
{
    public string OrderNumber { get; set; } = "";
    public List<OrderLineDto> Lines { get; set; } = new();
}

// ---- DbContext ----

public class AppDb : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=:memory:");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasOne(c => c.Address).WithOne().HasForeignKey<Address>(a => a.CustomerId);
        modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
        modelBuilder.Entity<OrderLine>().HasOne<Order>().WithMany(o => o.Lines).HasForeignKey(l => l.OrderId);
    }
}

// ---- OpenAutoMapper Profile ----

public class EfProfile : Profile
{
    public EfProfile()
    {
        CreateProjection<Customer, CustomerDto>();
        CreateProjection<Address, AddressDto>();

        // CustomerName is auto-flattened from Customer.Name by convention
        // LineCount: complex navigation expressions (s.Lines.Count) are not yet
        // supported by the compile-time generator — ignore for now
        CreateProjection<Order, OrderSummaryDto>()
            .Ignore(d => d.LineCount);

        CreateProjection<Order, OrderDetailDto>();
        CreateProjection<OrderLine, OrderLineDto>();
    }
}
