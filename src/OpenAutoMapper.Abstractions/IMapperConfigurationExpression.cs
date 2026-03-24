#nullable enable

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Reflection;

namespace OpenAutoMapper;

/// <summary>
/// Top-level configuration expression for registering mappings and profiles.
/// </summary>
public interface IMapperConfigurationExpression
{
    IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

    IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList);

    IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>();

    IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(MemberList memberList);

    void AddProfile(Profile profile);

    void AddProfile<TProfile>() where TProfile : Profile, new();

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode("Scans assemblies for Profile types using reflection. Use AddProfile<T>() for trim-safe registration.")]
#endif
    void AddMaps(params Assembly[] assemblies);
}
