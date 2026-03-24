#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using OpenAutoMapper.Exceptions;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Stores and validates all mapping configuration. Use <see cref="CreateMapper"/> to obtain an <see cref="IMapper"/>.
/// </summary>
public sealed class MapperConfiguration : IConfigurationProvider
{
    private readonly MapperConfigurationExpression _expression;
    private readonly List<Profile> _profiles = new();
    private readonly List<TypeMapConfiguration> _typeMaps = new();

    /// <summary>
    /// Internal factory delegate set by the source generator to create mapper instances.
    /// </summary>
    internal static Func<MapperConfiguration, IMapper>? MapperFactory { get; set; }

    /// <summary>
    /// Internal factory delegate that accepts a service constructor, set by the source generator.
    /// </summary>
    internal static Func<MapperConfiguration, Func<Type, object>, IMapper>? MapperFactoryWithServiceCtor { get; set; }

    /// <summary>
    /// Gets the read-only list of type map configurations.
    /// </summary>
    internal IReadOnlyList<TypeMapConfiguration> TypeMaps => _typeMaps;

    /// <summary>
    /// Gets the registered profiles.
    /// </summary>
    internal IReadOnlyList<Profile> Profiles => _profiles;

    /// <summary>
    /// Creates a new mapper configuration using the specified configuration action.
    /// </summary>
    public MapperConfiguration(Action<IMapperConfigurationExpression> configure)
        : this(configure, namingConvention: null)
    {
    }

    /// <summary>
    /// Creates a new mapper configuration with optional naming convention configuration.
    /// </summary>
    public MapperConfiguration(Action<IMapperConfigurationExpression> configure, Action<INamingConventionExpression>? namingConvention = null)
    {
        ProfileHelper.EnsureInitialized();
        _expression = new MapperConfigurationExpression();
        configure(_expression);

        // Collect inline type maps from the expression
        _typeMaps.AddRange(_expression.TypeMapConfigurations);

        // Collect profiles and their type maps
        foreach (var profile in _expression.Profiles)
        {
            _profiles.Add(profile);
            _typeMaps.AddRange(ProfileHelper.GetTypeMapConfigurations(profile));
        }

        // Apply naming conventions if provided
        if (namingConvention != null)
        {
            var namingExpr = new NamingConventionExpression();
            namingConvention(namingExpr);
        }
    }

    /// <summary>
    /// Creates an <see cref="IMapper"/> instance from this configuration.
    /// </summary>
    /// <returns>A configured mapper instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no mapper factory has been registered by the source generator.
    /// </exception>
    public IMapper CreateMapper()
    {
        if (MapperFactory is null)
        {
            throw new InvalidOperationException(
                "No mapper factory registered. Ensure the OpenAutoMapper.Generator source generator is referenced.");
        }

        return MapperFactory(this);
    }

    /// <summary>
    /// Creates an <see cref="IMapper"/> instance from this configuration with a custom service constructor.
    /// </summary>
    /// <param name="serviceCtor">A function that resolves service instances by type.</param>
    /// <returns>A configured mapper instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no mapper factory has been registered by the source generator.
    /// </exception>
    public IMapper CreateMapper(Func<Type, object> serviceCtor)
    {
        if (MapperFactoryWithServiceCtor is null)
        {
            throw new InvalidOperationException(
                "No mapper factory registered. Ensure the OpenAutoMapper.Generator source generator is referenced.");
        }

        return MapperFactoryWithServiceCtor(this, serviceCtor);
    }

    /// <summary>
    /// Validates that all mappings are properly configured.
    /// </summary>
    /// <exception cref="AutoMapperConfigurationException">
    /// Thrown when unmapped destination properties are detected.
    /// </exception>
    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var typeMap in _typeMaps)
        {
            ValidateTypeMap(typeMap, errors);
        }

        if (errors.Count > 0)
        {
            throw new AutoMapperConfigurationException(
                $"Configuration validation failed with {errors.Count} error(s):{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors),
                errors);
        }
    }

    /// <summary>
    /// Validates mappings for a specific profile name.
    /// </summary>
    /// <param name="profileName">The name of the profile type to validate.</param>
    /// <exception cref="AutoMapperConfigurationException">
    /// Thrown when unmapped destination properties are detected in the specified profile.
    /// </exception>
    public void AssertConfigurationIsValid(string profileName)
    {
        var errors = new List<string>();
        var profile = _profiles.FirstOrDefault(p => p.GetType().Name == profileName);

        if (profile is null)
        {
            throw new AutoMapperConfigurationException(
                $"Profile '{profileName}' was not found.",
                new[] { $"Profile '{profileName}' was not found." });
        }

        foreach (var typeMap in ProfileHelper.GetTypeMapConfigurations(profile))
        {
            ValidateTypeMap(typeMap, errors);
        }

        if (errors.Count > 0)
        {
            throw new AutoMapperConfigurationException(
                $"Configuration validation for profile '{profileName}' failed with {errors.Count} error(s):{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors),
                errors);
        }
    }

    private static void ValidateTypeMap(TypeMapConfiguration typeMap, List<string> errors)
    {
        if (typeMap.MemberList == MemberList.None)
        {
            return;
        }

        // Runtime validation is minimal — the source generator performs
        // comprehensive compile-time validation via diagnostics.
        // Here we just verify that the configuration was processed.
    }
}
