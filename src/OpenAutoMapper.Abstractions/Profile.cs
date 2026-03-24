#nullable enable

using System;
using System.Collections.Generic;

namespace OpenAutoMapper;

/// <summary>
/// Provides generic factory methods for creating mapping/projection expressions.
/// Implemented by Core to avoid reflection-based generic type construction.
/// </summary>
internal interface IProfileExpressionFactory
{
    IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(
        MemberList memberList,
        IList<object> configList);

    IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(
        MemberList memberList,
        IList<object> configList);
}

/// <summary>
/// Base class for mapping profile definitions. Derive from this class
/// to organize mapping configurations into logical groups.
/// </summary>
public abstract class Profile
{
    private readonly List<object> _typeMapConfigurations = new();
    private readonly List<string> _prefixes = new();
    private readonly List<string> _postfixes = new();
    private readonly List<string> _globalIgnores = new();

    /// <summary>
    /// When set to true, null source collections map to null instead of an empty collection.
    /// </summary>
    protected bool AllowNullCollections { get; set; }

    /// <summary>
    /// Internal factory set by Core to create mapping expressions without reflection.
    /// </summary>
    internal static IProfileExpressionFactory? ExpressionFactory { get; set; }

    /// <summary>
    /// Gets the list of type map configurations registered in this profile (stored as objects).
    /// </summary>
    internal IList<object> TypeMapConfigurationsUntyped => _typeMapConfigurations;

    /// <summary>
    /// Gets the recognized prefixes for this profile.
    /// </summary>
    internal List<string> Prefixes => _prefixes;

    /// <summary>
    /// Gets the recognized postfixes for this profile.
    /// </summary>
    internal List<string> Postfixes => _postfixes;

    /// <summary>
    /// Gets the global ignore patterns for this profile.
    /// </summary>
    internal List<string> GlobalIgnores => _globalIgnores;

    /// <summary>
    /// Creates a mapping between a source and destination type.
    /// </summary>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        return CreateMap<TSource, TDestination>(MemberList.Destination);
    }

    /// <summary>
    /// Creates a named mapping between a source and destination type.
    /// </summary>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string name)
    {
        return CreateMap<TSource, TDestination>(name, MemberList.Destination);
    }

    /// <summary>
    /// Creates a named mapping between a source and destination type with a specific member list validation.
    /// </summary>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string name, MemberList memberList)
    {
        if (ExpressionFactory is null)
        {
            throw new InvalidOperationException(
                "Profile.CreateMap requires the OpenAutoMapper.Core assembly to be loaded. " +
                "Ensure MapperConfiguration is constructed before Profile methods are called.");
        }

        return ExpressionFactory.CreateMap<TSource, TDestination>(memberList, _typeMapConfigurations);
    }

    /// <summary>
    /// Creates a mapping between a source and destination type with a specific member list validation.
    /// </summary>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
    {
        if (ExpressionFactory is null)
        {
            throw new InvalidOperationException(
                "Profile.CreateMap requires the OpenAutoMapper.Core assembly to be loaded. " +
                "Ensure MapperConfiguration is constructed before Profile methods are called.");
        }

        return ExpressionFactory.CreateMap<TSource, TDestination>(memberList, _typeMapConfigurations);
    }

    /// <summary>
    /// Creates an expression-based projection between a source and destination type.
    /// </summary>
    protected IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>()
    {
        return CreateProjection<TSource, TDestination>(MemberList.Destination);
    }

    /// <summary>
    /// Creates an expression-based projection between a source and destination type with a specific member list validation.
    /// </summary>
    protected IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(MemberList memberList)
    {
        if (ExpressionFactory is null)
        {
            throw new InvalidOperationException(
                "Profile.CreateProjection requires the OpenAutoMapper.Core assembly to be loaded. " +
                "Ensure MapperConfiguration is constructed before Profile methods are called.");
        }

        return ExpressionFactory.CreateProjection<TSource, TDestination>(memberList, _typeMapConfigurations);
    }

    /// <summary>
    /// Specifies prefixes to recognize when matching source to destination members.
    /// </summary>
    protected void RecognizePrefixes(params string[] prefixes)
    {
        _prefixes.AddRange(prefixes);
    }

    /// <summary>
    /// Specifies postfixes to recognize when matching source to destination members.
    /// </summary>
    protected void RecognizePostfixes(params string[] postfixes)
    {
        _postfixes.AddRange(postfixes);
    }

    /// <summary>
    /// Adds a global ignore rule for properties whose names start with the given string.
    /// </summary>
    protected void AddGlobalIgnore(string propertyNameStartingWith)
    {
        _globalIgnores.Add(propertyNameStartingWith);
    }
}
