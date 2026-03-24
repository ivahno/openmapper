#nullable enable

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Internal fluent builder that records mapping configuration into a <see cref="TypeMapConfiguration"/>.
/// </summary>
internal sealed class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
{
    private readonly TypeMapConfiguration _typeMapConfiguration;
    private readonly IList<object> _allConfigurations;

    internal MappingExpression(TypeMapConfiguration typeMapConfiguration, IList<object> allConfigurations)
    {
        _typeMapConfiguration = typeMapConfiguration;
        _allConfigurations = allConfigurations;
    }

    public IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        var memberExpr = new MemberConfigurationExpression<TSource, TDestination, TMember>(propertyMap);
        memberOptions(memberExpr);
        return this;
    }

    public IMappingExpression<TSource, TDestination> ForPath<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var path = GetMemberPath(destinationMember);
        var pathMap = new PathMap(path);
        _typeMapConfiguration.PathMaps.Add(pathMap);

        // Also create a property map for the path member options
        var propertyMap = new PropertyMap { DestinationMemberName = path };
        var memberExpr = new MemberConfigurationExpression<TSource, TDestination, TMember>(propertyMap);
        memberOptions(memberExpr);
        pathMap.SourceExpression = propertyMap.CustomMapExpression;
        return this;
    }

    public IMappingExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        propertyMap.IsIgnored = true;
        return this;
    }

    public IMappingExpression<TSource, TDestination> MapFrom<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Expression<Func<TSource, TMember>> sourceMember)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        propertyMap.CustomMapExpression = sourceMember;
        propertyMap.SourceMemberName = GetMemberName(sourceMember);
        return this;
    }

    public IMappingExpression<TSource, TDestination> Condition(Func<TSource, TDestination, bool> condition)
    {
        _typeMapConfiguration.Condition = condition;
        return this;
    }

    public IMappingExpression<TSource, TDestination> PreCondition(Func<TSource, bool> condition)
    {
        _typeMapConfiguration.PreCondition = condition;
        return this;
    }

    public IMappingExpression<TSource, TDestination> NullSubstitute(object value)
    {
        _typeMapConfiguration.NullSubstitute = value;
        return this;
    }

    public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
    {
        _typeMapConfiguration.ConstructUsing = ctor;
        return this;
    }

    public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
    {
        _typeMapConfiguration.BeforeMap = beforeFunction;
        return this;
    }

    public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
    {
        _typeMapConfiguration.AfterMap = afterFunction;
        return this;
    }

    public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
        where TOtherSource : TSource
        where TOtherDestination : TDestination
    {
        _typeMapConfiguration.IncludedDerivedTypes.Add(new TypePair(typeof(TOtherSource), typeof(TOtherDestination)));
        return this;
    }

    public IMappingExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class
    {
        _typeMapConfiguration.IncludedBaseTypes.Add(new TypePair(typeof(TBaseSource), typeof(TBaseDestination)));
        return this;
    }

    public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
    {
        _typeMapConfiguration.MaxDepth = depth;
        return this;
    }

    public IMappingExpression<TDestination, TSource> ReverseMap()
    {
        var reverseConfig = new TypeMapConfiguration(
            typeof(TDestination),
            typeof(TSource),
            _typeMapConfiguration.MemberList,
            isProjection: _typeMapConfiguration.IsProjection);

        _typeMapConfiguration.ReverseMapConfiguration = reverseConfig;
        _allConfigurations.Add(reverseConfig);

        return new MappingExpression<TDestination, TSource>(reverseConfig, _allConfigurations);
    }

    public IMappingExpression<TSource, TDestination> ConvertUsing(ITypeConverter<TSource, TDestination> converter)
    {
        _typeMapConfiguration.ConvertUsing = converter;
        return this;
    }

    public IMappingExpression<TSource, TDestination> ForAllMembers(
        Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        var propertyMap = new PropertyMap { DestinationMemberName = "*" };
        var memberExpr = new MemberConfigurationExpression<TSource, TDestination, object>(propertyMap);
        memberOptions(memberExpr);
        _typeMapConfiguration.ForAllMembersConfig = propertyMap;
        return this;
    }

    public IMappingExpression<TSource, TDestination> IncludeMembers(
        params Expression<Func<TSource, object>>[] membersToInclude)
    {
        foreach (var member in membersToInclude)
        {
            var memberName = GetMemberName(member);
            _typeMapConfiguration.IncludedMemberNames.Add(memberName);
        }
        return this;
    }

    public IMappingExpression<TSource, TDestination> UseDeepCloning()
    {
        _typeMapConfiguration.IsDeepClone = true;
        return this;
    }

    public IMappingExpression<TSource, TDestination> IncludeSource<TOther>()
    {
        _typeMapConfiguration.AdditionalSourceTypes.Add(typeof(TOther));
        return this;
    }

    public IMappingExpression<TSource, TDestination> ForCtorParam(
        string ctorParamName,
        Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
    {
        var ctorExpr = new CtorParamConfigurationExpression<TSource>(ctorParamName, _typeMapConfiguration);
        paramOptions(ctorExpr);
        return this;
    }

    private PropertyMap GetOrCreatePropertyMap(string memberName)
    {
        var existing = _typeMapConfiguration.PropertyMaps.Find(pm => pm.DestinationMemberName == memberName);
        if (existing != null)
        {
            return existing;
        }

        var propertyMap = new PropertyMap { DestinationMemberName = memberName };
        _typeMapConfiguration.PropertyMaps.Add(propertyMap);
        return propertyMap;
    }

    private static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression.Body is UnaryExpression { Operand: MemberExpression unaryMember })
        {
            return unaryMember.Member.Name;
        }

        throw new ArgumentException($"Expression '{expression}' does not refer to a member.", nameof(expression));
    }

    private static string GetMemberPath<T, TMember>(Expression<Func<T, TMember>> expression)
    {
        var parts = new List<string>();
        var current = expression.Body;

        while (current is MemberExpression memberExpr)
        {
            parts.Insert(0, memberExpr.Member.Name);
            current = memberExpr.Expression;
        }

        if (parts.Count == 0)
        {
            throw new ArgumentException($"Expression '{expression}' does not refer to a member path.", nameof(expression));
        }

        return string.Join(".", parts);
    }
}
