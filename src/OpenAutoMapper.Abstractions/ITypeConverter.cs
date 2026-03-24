#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Custom type converter for converting an entire source type to a destination type.
/// </summary>
public interface ITypeConverter<in TSource, TDestination>
{
    TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
}
