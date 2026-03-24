#nullable enable

using System;
using System.Linq.Expressions;

namespace OpenAutoMapper;

/// <summary>
/// Expression-safe subset of <see cref="IMappingExpression{TSource, TDestination}"/>
/// for use with IQueryable projections.
/// </summary>
public interface IProjectionExpression<TSource, TDestination>
{
    IProjectionExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

    IProjectionExpression<TSource, TDestination> ForPath<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

    IProjectionExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember);

    IProjectionExpression<TSource, TDestination> MapFrom<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Expression<Func<TSource, TMember>> sourceMember);
}
