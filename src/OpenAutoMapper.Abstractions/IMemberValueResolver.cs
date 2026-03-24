#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Custom value resolver that also receives the source member value.
/// </summary>
public interface IMemberValueResolver<in TSource, in TDestination, in TSourceMember, TDestMember>
{
    TDestMember Resolve(TSource source, TDestination destination, TSourceMember sourceMember, TDestMember destMember, ResolutionContext context);
}
