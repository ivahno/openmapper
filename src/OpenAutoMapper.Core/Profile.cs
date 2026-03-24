#nullable enable

using System.Collections.Generic;
using System.Linq;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Implements the <see cref="IProfileExpressionFactory"/> to bridge Profile (Abstractions)
/// with Core's TypeMapConfiguration and expression types.
/// Uses generic methods to avoid MakeGenericType and Activator.CreateInstance,
/// making it fully AOT-compatible.
/// </summary>
internal sealed class CoreProfileExpressionFactory : IProfileExpressionFactory
{
    internal static readonly CoreProfileExpressionFactory Instance = new();

    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(
        MemberList memberList,
        IList<object> configList)
    {
        var typeMapConfig = new TypeMapConfiguration(typeof(TSource), typeof(TDestination), memberList, isProjection: false);
        configList.Add(typeMapConfig);
        return new MappingExpression<TSource, TDestination>(typeMapConfig, configList);
    }

    public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(
        MemberList memberList,
        IList<object> configList)
    {
        var typeMapConfig = new TypeMapConfiguration(typeof(TSource), typeof(TDestination), memberList, isProjection: true);
        configList.Add(typeMapConfig);
        return new ProjectionExpression<TSource, TDestination>(typeMapConfig);
    }
}

/// <summary>
/// Helper for accessing Profile type map configurations from Core.
/// </summary>
internal static class ProfileHelper
{
    /// <summary>
    /// Ensures the expression factory is registered on the Profile class.
    /// </summary>
    internal static void EnsureInitialized()
    {
        Profile.ExpressionFactory ??= CoreProfileExpressionFactory.Instance;
    }

    /// <summary>
    /// Gets the typed TypeMapConfiguration list from a Profile's untyped storage.
    /// </summary>
    internal static List<TypeMapConfiguration> GetTypeMapConfigurations(Profile profile)
    {
        return profile.TypeMapConfigurationsUntyped
            .OfType<TypeMapConfiguration>()
            .ToList();
    }
}
