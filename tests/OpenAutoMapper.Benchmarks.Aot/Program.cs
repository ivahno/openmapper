using System.Diagnostics;
using OpenAutoMapper.Benchmarks.Aot;

var flatSource = new OrderSource
{
    Id = 1, OrderNumber = "ORD-001", CustomerName = "John Doe",
    CustomerEmail = "john@example.com", Amount = 99.99m, Tax = 8.50m,
    Discount = 5.00m, Currency = "USD", Notes = "Test order", IsActive = true
};

var nestedSource = new CustomerSource
{
    Id = 1, Name = "John Doe",
    Address = new AddressSource
    {
        Street = "123 Main St", City = "Springfield", State = "IL", Zip = "62701"
    }
};

const int warmup = 100_000;
const int iterations = 5_000_000;

Console.WriteLine("=== OpenAutoMapper AOT Performance Benchmark ===");
Console.WriteLine($"Mode: {(System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "NativeAOT")}");
Console.WriteLine($"Iterations: {iterations:N0}");
Console.WriteLine();

// Warmup all paths
for (int i = 0; i < warmup; i++)
{
    _ = new OrderDest
    {
        Id = flatSource.Id, OrderNumber = flatSource.OrderNumber,
        CustomerName = flatSource.CustomerName, CustomerEmail = flatSource.CustomerEmail,
        Amount = flatSource.Amount, Tax = flatSource.Tax, Discount = flatSource.Discount,
        Currency = flatSource.Currency, Notes = flatSource.Notes, IsActive = flatSource.IsActive
    };
    _ = flatSource.MapToOrderDest();
    _ = AotMapperlyMapper.MapOrder(flatSource);
}

// ---- Flat DTO Benchmark ----
Console.WriteLine("--- Flat DTO (10 properties) ---");

var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    _ = new OrderDest
    {
        Id = flatSource.Id, OrderNumber = flatSource.OrderNumber,
        CustomerName = flatSource.CustomerName, CustomerEmail = flatSource.CustomerEmail,
        Amount = flatSource.Amount, Tax = flatSource.Tax, Discount = flatSource.Discount,
        Currency = flatSource.Currency, Notes = flatSource.Notes, IsActive = flatSource.IsActive
    };
}
sw.Stop();
var handWrittenNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  Hand-written:        {handWrittenNs,8:F2} ns/op");

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = flatSource.MapToOrderDest();
}
sw.Stop();
var oamNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  OpenAutoMapper:      {oamNs,8:F2} ns/op  ({oamNs / handWrittenNs:F2}x baseline)");

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = AotMapperlyMapper.MapOrder(flatSource);
}
sw.Stop();
var mapperlyNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  Mapperly:            {mapperlyNs,8:F2} ns/op  ({mapperlyNs / handWrittenNs:F2}x baseline)");

// ---- Nested Benchmark ----
Console.WriteLine();
Console.WriteLine("--- Nested DTO (with Address) ---");

for (int i = 0; i < warmup; i++)
{
    _ = nestedSource.MapToCustomerDest();
    _ = AotMapperlyMapper.MapCustomer(nestedSource);
}

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = new CustomerDest
    {
        Id = nestedSource.Id, Name = nestedSource.Name,
        Address = nestedSource.Address is not null ? new AddressDest
        {
            Street = nestedSource.Address.Street, City = nestedSource.Address.City,
            State = nestedSource.Address.State, Zip = nestedSource.Address.Zip
        } : null
    };
}
sw.Stop();
handWrittenNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  Hand-written:        {handWrittenNs,8:F2} ns/op");

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = nestedSource.MapToCustomerDest();
}
sw.Stop();
oamNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  OpenAutoMapper:      {oamNs,8:F2} ns/op  ({oamNs / handWrittenNs:F2}x baseline)");

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = AotMapperlyMapper.MapCustomer(nestedSource);
}
sw.Stop();
mapperlyNs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000 / iterations;
Console.WriteLine($"  Mapperly:            {mapperlyNs,8:F2} ns/op  ({mapperlyNs / handWrittenNs:F2}x baseline)");

Console.WriteLine();
Console.WriteLine("Note: AutoMapper and Mapster are excluded — they require");
Console.WriteLine("      runtime reflection/code generation incompatible with NativeAOT.");
