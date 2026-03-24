#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Action invoked before or after a mapping operation.
/// </summary>
public interface IMappingAction<in TSource, in TDestination>
{
    void Process(TSource source, TDestination destination, ResolutionContext context);
}
