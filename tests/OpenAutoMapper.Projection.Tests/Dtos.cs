namespace OpenAutoMapper.Projection.Tests;

// ---- DTOs ----

/// <summary>Simple flat projection from Customer.</summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

/// <summary>Customer with nested address (tests nested object projection).</summary>
public class CustomerWithAddressDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AddressDto? Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
}

/// <summary>Order summary with navigation property flattening.</summary>
public class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";
    public int LineCount { get; set; }
}

/// <summary>Order with nested collection of line items.</summary>
public class OrderWithLinesDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public List<OrderLineDto> Lines { get; set; } = new();
}

public class OrderLineDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
