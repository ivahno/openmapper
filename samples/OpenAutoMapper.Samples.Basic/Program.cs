using OpenAutoMapper;
using OpenAutoMapper.Samples.Basic;

var source = new Source { Id = 1, Name = "Test", Value = 42.5m };

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

var mapper = config.CreateMapper();
var dest = mapper.Map<Source, Dest>(source);

Console.WriteLine("OpenAutoMapper Basic Sample");
Console.WriteLine($"Source: Id={source.Id}, Name={source.Name}, Value={source.Value}");
Console.WriteLine($"Mapped: Id={dest.Id}, Name={dest.Name}, Value={dest.Value}");

namespace OpenAutoMapper.Samples.Basic
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class Dest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Source, Dest>();
        }
    }
}
