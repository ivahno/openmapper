#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Internal implementation of <see cref="IMapperConfigurationExpression"/> that collects mapping registrations.
/// </summary>
internal sealed class MapperConfigurationExpression : IMapperConfigurationExpression
{
    private readonly List<Profile> _profiles = new();
    private readonly List<object> _typeMapConfigurations = new();

    /// <summary>
    /// Gets the profiles registered through this expression.
    /// </summary>
    internal IReadOnlyList<Profile> Profiles => _profiles;

    /// <summary>
    /// Gets the inline type map configurations registered through this expression.
    /// </summary>
    internal IReadOnlyList<TypeMapConfiguration> TypeMapConfigurations =>
        _typeMapConfigurations.Cast<TypeMapConfiguration>().ToList();

    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        return CreateMap<TSource, TDestination>(MemberList.Destination);
    }

    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
    {
        var typeMapConfig = new TypeMapConfiguration(typeof(TSource), typeof(TDestination), memberList, isProjection: false);
        _typeMapConfigurations.Add(typeMapConfig);
        return new MappingExpression<TSource, TDestination>(typeMapConfig, _typeMapConfigurations);
    }

    public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>()
    {
        return CreateProjection<TSource, TDestination>(MemberList.Destination);
    }

    public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(MemberList memberList)
    {
        var typeMapConfig = new TypeMapConfiguration(typeof(TSource), typeof(TDestination), memberList, isProjection: true);
        _typeMapConfigurations.Add(typeMapConfig);
        return new ProjectionExpression<TSource, TDestination>(typeMapConfig);
    }

    public void AddProfile(Profile profile)
    {
        _profiles.Add(profile);
    }

    public void AddProfile<TProfile>() where TProfile : Profile, new()
    {
        _profiles.Add(new TProfile());
    }

    [RequiresUnreferencedCode("Scans assemblies for Profile types using reflection. Use AddProfile<T>() for trim-safe registration.")]
    public void AddMaps(params Assembly[] assemblies)
    {
        // Assembly scanning is inherently reflection-based. The source generator
        // overrides this at compile time. Suppress AOT/trim warnings for this method.
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access
#pragma warning disable IL2070 // 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor'
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMemberTypes' in call
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling
        foreach (var assembly in assemblies)
        {
            var profileTypes = assembly.GetTypes()
                .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

            foreach (var profileType in profileTypes)
            {
                var profile = (Profile)Activator.CreateInstance(profileType)!;
                _profiles.Add(profile);
            }
        }
#pragma warning restore IL3050
#pragma warning restore IL2072
#pragma warning restore IL2070
#pragma warning restore IL2026
    }
}
