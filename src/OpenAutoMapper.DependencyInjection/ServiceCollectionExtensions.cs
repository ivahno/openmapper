using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenAutoMapper;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up OpenAutoMapper services in an <see cref="IServiceCollection" />.
/// </summary>
public static class OpenAutoMapperServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAutoMapper services to the specified <see cref="IServiceCollection" />.
    /// Drop-in replacement for AutoMapper's AddAutoMapper().
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    public static IServiceCollection AddAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        return AddOpenAutoMapper(services, null, assemblies);
    }

    /// <summary>
    /// Adds OpenAutoMapper services with custom configuration.
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    public static IServiceCollection AddAutoMapper(
        this IServiceCollection services,
        Action<IMapperConfigurationExpression> configAction,
        params Assembly[] assemblies)
    {
        return AddOpenAutoMapper(services, configAction, assemblies);
    }

    /// <summary>
    /// Adds OpenAutoMapper services using profile marker types for assembly scanning.
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    public static IServiceCollection AddAutoMapper(this IServiceCollection services, params Type[] profileAssemblyMarkerTypes)
    {
        var assemblies = profileAssemblyMarkerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return AddOpenAutoMapper(services, null, assemblies);
    }

    /// <summary>
    /// Adds OpenAutoMapper services. Preferred name for new projects.
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    public static IServiceCollection AddOpenAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        return AddOpenAutoMapper(services, null, assemblies);
    }

    /// <summary>
    /// Adds a keyed OpenAutoMapper configuration for scenarios with multiple mapper instances.
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    public static IServiceCollection AddKeyedOpenAutoMapper(
        this IServiceCollection services,
        string serviceKey,
        Action<IMapperConfigurationExpression> configAction,
        params Assembly[] assemblies)
    {
        var config = new MapperConfiguration(cfg =>
        {
            configAction.Invoke(cfg);
            if (assemblies.Length > 0)
            {
                cfg.AddMaps(assemblies);
            }
        });

        services.AddKeyedSingleton<IConfigurationProvider>(serviceKey, config);
        services.AddKeyedSingleton<MapperConfiguration>(serviceKey, config);
        services.AddKeyedSingleton<IMapper>(serviceKey, (sp, _) => config.CreateMapper(sp.GetService!));

        return services;
    }

    [RequiresUnreferencedCode("Assembly scanning uses reflection. Use AddProfile<T>() for trim-safe registration.")]
    private static IServiceCollection AddOpenAutoMapper(
        IServiceCollection services,
        Action<IMapperConfigurationExpression>? configAction,
        Assembly[] assemblies)
    {
        var config = new MapperConfiguration(cfg =>
        {
            configAction?.Invoke(cfg);
            if (assemblies.Length > 0)
            {
                cfg.AddMaps(assemblies);
            }
        });

        services.AddSingleton<IConfigurationProvider>(config);
        services.AddSingleton<MapperConfiguration>(config);
        services.AddSingleton<IMapper>(sp => config.CreateMapper(sp.GetService!));

        return services;
    }
}
