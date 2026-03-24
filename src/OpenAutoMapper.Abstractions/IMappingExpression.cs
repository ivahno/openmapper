#nullable enable

using System;
using System.Linq.Expressions;

namespace OpenAutoMapper;

/// <summary>
/// Fluent API for configuring a mapping between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.
/// </summary>
public interface IMappingExpression<TSource, TDestination>
{
    IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

    IMappingExpression<TSource, TDestination> ForPath<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

    IMappingExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember);

    IMappingExpression<TSource, TDestination> MapFrom<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Expression<Func<TSource, TMember>> sourceMember);

    IMappingExpression<TSource, TDestination> Condition(
        Func<TSource, TDestination, bool> condition);

    IMappingExpression<TSource, TDestination> PreCondition(
        Func<TSource, bool> condition);

    IMappingExpression<TSource, TDestination> NullSubstitute(object value);

    IMappingExpression<TSource, TDestination> ConstructUsing(
        Func<TSource, TDestination> ctor);

    IMappingExpression<TSource, TDestination> BeforeMap(
        Action<TSource, TDestination> beforeFunction);

    IMappingExpression<TSource, TDestination> AfterMap(
        Action<TSource, TDestination> afterFunction);

    IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
        where TOtherSource : TSource
        where TOtherDestination : TDestination;

    IMappingExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class;

    IMappingExpression<TSource, TDestination> MaxDepth(int depth);

    IMappingExpression<TSource, TDestination> ForCtorParam(
        string ctorParamName,
        Action<ICtorParamConfigurationExpression<TSource>> paramOptions);

    IMappingExpression<TDestination, TSource> ReverseMap();

    IMappingExpression<TSource, TDestination> ConvertUsing(
        ITypeConverter<TSource, TDestination> converter);

    IMappingExpression<TSource, TDestination> ForAllMembers(
        Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);

    IMappingExpression<TSource, TDestination> IncludeMembers(
        params Expression<Func<TSource, object>>[] membersToInclude);

    IMappingExpression<TSource, TDestination> UseDeepCloning();

    IMappingExpression<TSource, TDestination> IncludeSource<TOther>();
}
