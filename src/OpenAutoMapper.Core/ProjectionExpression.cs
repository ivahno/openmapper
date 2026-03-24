#nullable enable

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Internal expression-safe subset for IQueryable projections.
/// </summary>
internal sealed class ProjectionExpression<TSource, TDestination> : IProjectionExpression<TSource, TDestination>
{
    private readonly TypeMapConfiguration _typeMapConfiguration;

    internal ProjectionExpression(TypeMapConfiguration typeMapConfiguration)
    {
        _typeMapConfiguration = typeMapConfiguration;
    }

    public IProjectionExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        var memberExpr = new MemberConfigurationExpression<TSource, TDestination, TMember>(propertyMap);
        memberOptions(memberExpr);
        return this;
    }

    public IProjectionExpression<TSource, TDestination> ForPath<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var path = GetMemberPath(destinationMember);
        var pathMap = new PathMap(path);
        _typeMapConfiguration.PathMaps.Add(pathMap);

        var propertyMap = new PropertyMap { DestinationMemberName = path };
        var memberExpr = new MemberConfigurationExpression<TSource, TDestination, TMember>(propertyMap);
        memberOptions(memberExpr);
        pathMap.SourceExpression = propertyMap.CustomMapExpression;
        return this;
    }

    public IProjectionExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        propertyMap.IsIgnored = true;
        return this;
    }

    public IProjectionExpression<TSource, TDestination> MapFrom<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Expression<Func<TSource, TMember>> sourceMember)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = GetOrCreatePropertyMap(memberName);
        propertyMap.CustomMapExpression = sourceMember;
        propertyMap.SourceMemberName = GetMemberName(sourceMember);
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
