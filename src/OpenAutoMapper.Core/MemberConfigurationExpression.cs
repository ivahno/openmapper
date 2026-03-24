#nullable enable

using System;
using System.Linq.Expressions;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

/// <summary>
/// Internal implementation of per-member configuration options.
/// </summary>
internal sealed class MemberConfigurationExpression<TSource, TDestination, TMember>
    : IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    private readonly PropertyMap _propertyMap;

    internal MemberConfigurationExpression(PropertyMap propertyMap)
    {
        _propertyMap = propertyMap;
    }

    public void MapFrom(Expression<Func<TSource, TMember>> sourceMember)
    {
        _propertyMap.CustomMapExpression = sourceMember;

        if (sourceMember.Body is MemberExpression memberExpr)
        {
            _propertyMap.SourceMemberName = memberExpr.Member.Name;
        }
    }

    public void MapFrom<TValueResolver>()
        where TValueResolver : IValueResolver<TSource, TDestination, TMember>, new()
    {
        _propertyMap.ValueResolverType = typeof(TValueResolver);
    }

    public void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>, new()
    {
        _propertyMap.ValueResolverType = typeof(TValueResolver);

        if (sourceMember.Body is MemberExpression memberExpr)
        {
            _propertyMap.SourceMemberName = memberExpr.Member.Name;
        }
    }

    public void Ignore()
    {
        _propertyMap.IsIgnored = true;
    }

    public void Condition(Func<TSource, TDestination, TMember, bool> condition)
    {
        _propertyMap.Condition = condition;
    }

    public void PreCondition(Func<TSource, bool> condition)
    {
        _propertyMap.PreCondition = condition;
    }

    public void NullSubstitute(TMember value)
    {
        _propertyMap.NullSubstitute = value;
    }

    public void UseDestinationValue()
    {
        _propertyMap.UseDestinationValue = true;
    }
}
