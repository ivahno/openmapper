#nullable enable

using System;
using System.Linq.Expressions;

namespace OpenAutoMapper;

/// <summary>
/// Configuration expression for constructor parameter mapping.
/// </summary>
public interface ICtorParamConfigurationExpression<TSource>
{
    void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);
}
