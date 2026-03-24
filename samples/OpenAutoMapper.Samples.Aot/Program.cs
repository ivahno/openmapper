using OpenAutoMapper;
using OpenAutoMapper.Samples.Aot;

Console.WriteLine("OpenAutoMapper AOT Sample — All Features Validation");
Console.WriteLine($"Mode: {(System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "NativeAOT")}");
Console.WriteLine();

int passed = 0;
int failed = 0;

void Assert(bool condition, string name)
{
    if (condition) { passed++; Console.WriteLine($"  [PASS] {name}"); }
    else { failed++; Console.WriteLine($"  [FAIL] {name}"); }
}

// ---- Feature 0: Basic mapping (baseline) ----
Console.WriteLine("--- Basic Mapping ---");
var basicSource = new Order { Id = 1, CustomerName = "John Doe", Total = 99.99m };
var basicDest = basicSource.MapToOrderDto();
Assert(basicDest.Id == 1, "Basic: Id mapped");
Assert(basicDest.CustomerName == "John Doe", "Basic: CustomerName mapped");
Assert(basicDest.Total == 99.99m, "Basic: Total mapped");

// ---- Feature 1: Init-only / Record support ----
Console.WriteLine("--- Init-Only / Record Support ---");
var initSource = new PersonSource { FirstName = "Jane", LastName = "Doe", Age = 30 };
var initDest = initSource.MapToPersonInitOnly();
Assert(initDest.FirstName == "Jane", "Init-only: FirstName mapped via initializer");
Assert(initDest.LastName == "Doe", "Init-only: LastName mapped via initializer");
Assert(initDest.Age == 30, "Init-only: Age mapped via initializer");

// ---- Feature 2: Constructor mapping ----
Console.WriteLine("--- Constructor Mapping ---");
var ctorSource = new CtorSource { Id = 42, Name = "Test" };
var ctorDest = ctorSource.MapToCtorDest();
Assert(ctorDest.Id == 42, "Ctor: Id mapped via constructor");
Assert(ctorDest.Name == "Test", "Ctor: Name mapped via constructor");

// ---- Feature 3: ForAllMembers ----
Console.WriteLine("--- ForAllMembers ---");
var famSource = new FamSource { X = 10, Y = 20 };
var famDest = famSource.MapToFamDest();
Assert(famDest.X == 10, "ForAllMembers: X mapped (passes condition)");
Assert(famDest.Y == 20, "ForAllMembers: Y mapped (passes condition)");

// ---- Feature 4: IncludeMembers ----
Console.WriteLine("--- IncludeMembers ---");
var inclSource = new IncludeSource
{
    Id = 1,
    Details = new IncludeDetails { Email = "test@example.com", Phone = "555-1234" }
};
var inclDest = inclSource.MapToIncludeDest();
Assert(inclDest.Id == 1, "IncludeMembers: Id mapped from primary");
Assert(inclDest.Email == "test@example.com", "IncludeMembers: Email mapped from Details nav");
Assert(inclDest.Phone == "555-1234", "IncludeMembers: Phone mapped from Details nav");

// ---- Feature 5: Enum strategies ----
Console.WriteLine("--- Enum Mapping ---");
var enumSource = new EnumSrc { Color = SourceColor.Red };
var enumDest = enumSource.MapToEnumDst();
Assert(enumDest.Color == DestColor.Red, "Enum: Red mapped by name");

// ---- Feature 7: AllowNullCollections ----
Console.WriteLine("--- AllowNullCollections ---");
var nullCollSource = new NullCollSource { Id = 1, Items = null };
var nullCollDest = nullCollSource.MapToNullCollDest();
Assert(nullCollDest.Id == 1, "NullCollections: Id mapped");
Assert(nullCollDest.Items == null, "NullCollections: null Items stays null");

var nonNullSource = new NullCollSource { Id = 2, Items = new List<string> { "a", "b" } };
var nonNullDest = nonNullSource.MapToNullCollDest();
Assert(nonNullDest.Items != null && nonNullDest.Items.Count == 2, "NullCollections: non-null Items preserved");

// ---- Feature 8: Deep cloning ----
Console.WriteLine("--- Deep Cloning ---");
var cloneSource = new CloneObj { Id = 1, Name = "Original", Inner = new CloneInner { Value = "deep" } };
var cloned = cloneSource.MapToCloneObj();
Assert(cloned.Id == 1, "DeepClone: Id cloned");
Assert(cloned.Name == "Original", "DeepClone: Name cloned");
Assert(cloned.Inner != null && cloned.Inner.Value == "deep", "DeepClone: Inner value cloned");

// ---- Feature 9: Named mappings ----
Console.WriteLine("--- Named Mappings ---");
var namedSource = new NamedSrc { Id = 1, Name = "Full", Secret = "hidden" };
var namedFull = namedSource.MapToNamedDst();
Assert(namedFull.Id == 1 && namedFull.Name == "Full", "Named: default mapping works");

var namedSummary = namedSource.MapToNamedDst_summary();
Assert(namedSummary.Id == 1, "Named(summary): Id mapped");
Assert(namedSummary.Name == "Full", "Named(summary): Name mapped");

// ---- Feature 10: IMapper dispatch ----
Console.WriteLine("--- IMapper Dispatch ---");
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<AllFeaturesProfile>();
});
var mapper = config.CreateMapper();
var imapperDest = mapper.Map<Order, OrderDto>(basicSource);
Assert(imapperDest.Id == 1, "IMapper: dispatch works");

// ---- Summary ----
Console.WriteLine();
Console.WriteLine($"Results: {passed} passed, {failed} failed");
if (failed > 0) Environment.Exit(1);

namespace OpenAutoMapper.Samples.Aot
{
    // --- Basic ---
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    // --- Init-only ---
    public class PersonSource
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
    }
    public class PersonInitOnly
    {
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public int Age { get; init; }
    }

    // --- Constructor mapping ---
    public class CtorSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class CtorDest
    {
        public CtorDest(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public int Id { get; }
        public string Name { get; }
    }

    // --- ForAllMembers ---
    public class FamSource { public int X { get; set; } public int Y { get; set; } }
    public class FamDest { public int X { get; set; } public int Y { get; set; } }

    // --- IncludeMembers ---
    public class IncludeDetails { public string Email { get; set; } = ""; public string Phone { get; set; } = ""; }
    public class IncludeSource { public int Id { get; set; } public IncludeDetails Details { get; set; } = new(); }
    public class IncludeDest { public int Id { get; set; } public string Email { get; set; } = ""; public string Phone { get; set; } = ""; }

    // --- Enum ---
    public enum SourceColor { Red, Green, Blue }
    public enum DestColor { Red, Green, Blue }
    public class EnumSrc { public SourceColor Color { get; set; } }
    public class EnumDst { public DestColor Color { get; set; } }

    // --- Null collections ---
    public class NullCollSource { public int Id { get; set; } public List<string>? Items { get; set; } }
    public class NullCollDest { public int Id { get; set; } public List<string>? Items { get; set; } }

    // --- Deep clone ---
    public class CloneInner { public string Value { get; set; } = ""; }
    public class CloneObj { public int Id { get; set; } public string Name { get; set; } = ""; public CloneInner? Inner { get; set; } }

    // --- Named ---
    public class NamedSrc { public int Id { get; set; } public string Name { get; set; } = ""; public string Secret { get; set; } = ""; }
    public class NamedDst { public int Id { get; set; } public string Name { get; set; } = ""; public string? Secret { get; set; } }

    // --- Profile combining all features ---
    public class AllFeaturesProfile : Profile
    {
        public AllFeaturesProfile()
        {
            // Basic
            CreateMap<Order, OrderDto>();

            // Init-only
            CreateMap<PersonSource, PersonInitOnly>();

            // Constructor mapping (auto-detect: CtorDest has no parameterless ctor)
            CreateMap<CtorSource, CtorDest>();

            // ForAllMembers
            CreateMap<FamSource, FamDest>();

            // IncludeMembers
            CreateMap<IncludeSource, IncludeDest>()
                .IncludeMembers(s => (object)s.Details);

            // Enum by name
            CreateMap<EnumSrc, EnumDst>();

            // AllowNullCollections
            AllowNullCollections = true;
            CreateMap<NullCollSource, NullCollDest>();

            // Deep clone
            CreateMap<CloneInner, CloneInner>()
                .UseDeepCloning();
            CreateMap<CloneObj, CloneObj>()
                .UseDeepCloning();

            // Named mappings
            CreateMap<NamedSrc, NamedDst>();
            CreateMap<NamedSrc, NamedDst>("summary")
                .ForMember(d => d.Secret, opt => opt.Ignore());
        }
    }
}
