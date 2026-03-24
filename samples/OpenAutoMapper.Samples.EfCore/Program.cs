using Microsoft.EntityFrameworkCore;
using OpenAutoMapper.Samples.EfCore;

Console.WriteLine("=== OpenAutoMapper + EF Core Sample ===");
Console.WriteLine();

using var db = new AppDb();
db.Database.OpenConnection();
db.Database.EnsureCreated();

// Seed
var customer = new Customer
{
    Name = "Jane Doe",
    Email = "jane@example.com",
    Address = new Address { Street = "42 Oak Ave", City = "Portland", Zip = "97201" }
};
db.Customers.Add(customer);
db.SaveChanges();

db.Orders.AddRange(
    new Order
    {
        OrderNumber = "ORD-001",
        Total = 149.97m,
        CustomerId = customer.Id,
        Lines = new()
        {
            new() { ProductName = "Keyboard", Quantity = 1, UnitPrice = 79.99m },
            new() { ProductName = "Mouse", Quantity = 2, UnitPrice = 34.99m }
        }
    },
    new Order
    {
        OrderNumber = "ORD-002",
        Total = 25.00m,
        CustomerId = customer.Id,
        Lines = new() { new() { ProductName = "USB Cable", Quantity = 5, UnitPrice = 5.00m } }
    }
);
db.SaveChanges();

// 1. Nested object projection: Customer with Address
Console.WriteLine("--- Customer with nested Address ---");
foreach (var c in db.Customers.ProjectToCustomerDto().ToList())
    Console.WriteLine($"  {c.Name} — {c.Address?.City}, {c.Address?.Zip}");

// 2. Navigation flattening: Order → OrderSummaryDto
Console.WriteLine();
Console.WriteLine("--- Order summaries (navigation flattening) ---");
foreach (var o in db.Orders.OrderBy(o => o.OrderNumber).ProjectToOrderSummaryDto().ToList())
    Console.WriteLine($"  {o.OrderNumber}: ${o.Total} — {o.CustomerName}");

// 3. Nested collection: Order with line items
Console.WriteLine();
Console.WriteLine("--- Order detail with nested lines ---");
foreach (var o in db.Orders.Where(o => o.OrderNumber == "ORD-001").ProjectToOrderDetailDto().ToList())
{
    Console.WriteLine($"  {o.OrderNumber}:");
    foreach (var line in o.Lines)
        Console.WriteLine($"    {line.ProductName} x{line.Quantity} @ ${line.UnitPrice}");
}

// 4. Composable LINQ: filter + project + page
Console.WriteLine();
Console.WriteLine("--- Filtered & paged (Total > $50) ---");
foreach (var o in db.Orders.Where(o => o.Total > 50m).OrderByDescending(o => o.Total).ProjectToOrderSummaryDto().Take(5).ToList())
    Console.WriteLine($"  {o.OrderNumber}: ${o.Total}");

Console.WriteLine();
Console.WriteLine("Done!");
