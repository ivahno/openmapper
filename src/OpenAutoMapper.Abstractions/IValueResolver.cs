#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Custom value resolver for mapping a destination member from source context.
/// </summary>
public interface IValueResolver<in TSource, in TDestination, TDestMember>
{
    TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
}
