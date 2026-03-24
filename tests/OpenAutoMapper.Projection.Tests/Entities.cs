namespace OpenAutoMapper.Projection.Tests;

// ---- EF Core Entities ----

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
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
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
    public Order Order { get; set; } = null!;
}
