#nullable enable

using System;
using System.Linq.Expressions;

namespace OpenAutoMapper;

/// <summary>
/// Per-member configuration options within a mapping expression.
/// </summary>
public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    void MapFrom(Expression<Func<TSource, TMember>> sourceMember);

    void MapFrom<TValueResolver>()
        where TValueResolver : IValueResolver<TSource, TDestination, TMember>, new();

    void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>, new();

    void Ignore();

    void Condition(Func<TSource, TDestination, TMember, bool> condition);

    void PreCondition(Func<TSource, bool> condition);

    void NullSubstitute(TMember value);

    void UseDestinationValue();
}
