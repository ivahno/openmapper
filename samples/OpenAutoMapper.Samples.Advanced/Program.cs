using OpenAutoMapper;
using OpenAutoMapper.Samples.Advanced;

Console.WriteLine("=== OpenAutoMapper Advanced Sample ===");
Console.WriteLine();

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<AdvancedProfile>();
});

var mapper = config.CreateMapper();

// --- Polymorphic mapping ---
Console.WriteLine("--- Polymorphic Mapping ---");
Shape circle = new Circle { Name = "MyCircle", Radius = 5.0 };
var circleDto = mapper.Map<Shape, ShapeDto>(circle);
Console.WriteLine($"Circle mapped: Name={circleDto.Name}, Type={circleDto.GetType().Name}");

Shape rect = new Rectangle { Name = "MyRect", Width = 3.0, Height = 4.0 };
var rectDto = mapper.Map<Shape, ShapeDto>(rect);
Console.WriteLine($"Rectangle mapped: Name={rectDto.Name}, Type={rectDto.GetType().Name}");

// --- Circular reference with MaxDepth ---
Console.WriteLine();
Console.WriteLine("--- Circular Reference (MaxDepth=3) ---");
var ceo = new Employee { Id = 1, Name = "CEO", Manager = null };
var vp = new Employee { Id = 2, Name = "VP", Manager = ceo };
var dev = new Employee { Id = 3, Name = "Dev", Manager = vp };
ceo.Manager = ceo; // self-referencing

var devDto = mapper.Map<Employee, EmployeeDto>(dev);
Console.WriteLine($"Dev: {devDto.Name}, Manager: {devDto.Manager?.Name}, Manager.Manager: {devDto.Manager?.Manager?.Name}");

// --- Dictionary property ---
Console.WriteLine();
Console.WriteLine("--- Dictionary Mapping ---");
var product = new Product { Id = 1, Name = "Widget", Metadata = new() { ["color"] = "red", ["size"] = "large" } };
var productDto = mapper.Map<Product, ProductDto>(product);
Console.WriteLine($"Product: {productDto.Name}, Metadata count: {productDto.Metadata?.Count}");

// --- Enum switch ---
Console.WriteLine();
Console.WriteLine("--- Enum Mapping ---");
var task = new TaskItem { Id = 1, Title = "Fix bug", Priority = PriorityLevel.High };
var taskDto = mapper.Map<TaskItem, TaskItemDto>(task);
Console.WriteLine($"Task: {taskDto.Title}, Priority: {taskDto.Priority}");

// --- Collection mapping ---
Console.WriteLine();
Console.WriteLine("--- Collection Mapping ---");
var order = new Order
{
    Id = 1,
    Items = new()
    {
        new OrderItem { Product = "Apple", Qty = 3 },
        new OrderItem { Product = "Banana", Qty = 5 }
    }
};
var orderDto = mapper.Map<Order, OrderDto>(order);
Console.WriteLine($"Order: {orderDto.Id}, Items: {orderDto.Items?.Count}");
foreach (var item in orderDto.Items ?? new())
{
    Console.WriteLine($"  - {item.Product}: {item.Qty}");
}

// --- ConstructUsing ---
Console.WriteLine();
Console.WriteLine("--- ConstructUsing ---");
var record = new RecordSource { Id = 42, Label = "Test" };
var recordDto = mapper.Map<RecordSource, RecordDest>(record);
Console.WriteLine($"Record: Id={recordDto.Id}, Label={recordDto.Label}");

// --- AfterMap ---
Console.WriteLine();
Console.WriteLine("--- AfterMap ---");
var audit = new AuditSource { Id = 1, Value = "data" };
var auditDto = mapper.Map<AuditSource, AuditDest>(audit);
Console.WriteLine($"Audit: Id={auditDto.Id}, Value={auditDto.Value}, MappedAt set={auditDto.MappedAt != default}");

Console.WriteLine();
Console.WriteLine("All advanced scenarios completed successfully!");

namespace OpenAutoMapper.Samples.Advanced
{
    // --- Polymorphic ---
    public class Shape { public string Name { get; set; } = ""; }
    public class Circle : Shape { public double Radius { get; set; } }
    public class Rectangle : Shape { public double Width { get; set; } public double Height { get; set; } }

    public class ShapeDto { public string Name { get; set; } = ""; }
    public class CircleDto : ShapeDto { public double Radius { get; set; } }
    public class RectangleDto : ShapeDto { public double Width { get; set; } public double Height { get; set; } }

    // --- Circular Reference ---
    public class Employee { public int Id { get; set; } public string Name { get; set; } = ""; public Employee? Manager { get; set; } }
    public class EmployeeDto { public int Id { get; set; } public string Name { get; set; } = ""; public EmployeeDto? Manager { get; set; } }

    // --- Dictionary ---
    public class Product { public int Id { get; set; } public string Name { get; set; } = ""; public System.Collections.Generic.Dictionary<string, string>? Metadata { get; set; } }
    public class ProductDto { public int Id { get; set; } public string Name { get; set; } = ""; public System.Collections.Generic.Dictionary<string, string>? Metadata { get; set; } }

    // --- Enum ---
    public enum PriorityLevel { Low, Medium, High }
    public enum PriorityLevelDto { Low, Medium, High }
    public class TaskItem { public int Id { get; set; } public string Title { get; set; } = ""; public PriorityLevel Priority { get; set; } }
    public class TaskItemDto { public int Id { get; set; } public string Title { get; set; } = ""; public PriorityLevelDto Priority { get; set; } }

    // --- Collection ---
    public class OrderItem { public string Product { get; set; } = ""; public int Qty { get; set; } }
    public class OrderItemDto { public string Product { get; set; } = ""; public int Qty { get; set; } }
    public class Order { public int Id { get; set; } public System.Collections.Generic.List<OrderItem>? Items { get; set; } }
    public class OrderDto { public int Id { get; set; } public System.Collections.Generic.List<OrderItemDto>? Items { get; set; } }

    // --- ConstructUsing ---
    public class RecordSource { public int Id { get; set; } public string Label { get; set; } = ""; }
    public class RecordDest
    {
        public RecordDest(int id) { Id = id; }
        public int Id { get; set; }
        public string Label { get; set; } = "";
    }

    // --- AfterMap ---
    public class AuditSource { public int Id { get; set; } public string Value { get; set; } = ""; }
    public class AuditDest { public int Id { get; set; } public string Value { get; set; } = ""; public System.DateTime MappedAt { get; set; } }

    public class AdvancedProfile : Profile
    {
        public AdvancedProfile()
        {
            // Polymorphic
            CreateMap<Shape, ShapeDto>()
                .Include<Circle, CircleDto>()
                .Include<Rectangle, RectangleDto>();
            CreateMap<Circle, CircleDto>();
            CreateMap<Rectangle, RectangleDto>();

            // Circular reference
            CreateMap<Employee, EmployeeDto>()
                .MaxDepth(3);

            // Dictionary
            CreateMap<Product, ProductDto>();

            // Enum
            CreateMap<TaskItem, TaskItemDto>();

            // Collection
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();

            // ConstructUsing
            CreateMap<RecordSource, RecordDest>()
                .ConstructUsing(s => new RecordDest(s.Id));

            // AfterMap
            CreateMap<AuditSource, AuditDest>()
                .ForMember(d => d.MappedAt, opt => opt.Ignore())
                .AfterMap((s, d) => d.MappedAt = System.DateTime.UtcNow);
        }
    }
}
