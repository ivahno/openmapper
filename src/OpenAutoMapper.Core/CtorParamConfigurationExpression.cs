#nullable enable

using System;
using System.Linq.Expressions;
using OpenAutoMapper.Internal;

namespace OpenAutoMapper;

internal sealed class CtorParamConfigurationExpression<TSource> : ICtorParamConfigurationExpression<TSource>
{
    private readonly string _paramName;
    private readonly TypeMapConfiguration _config;

    internal CtorParamConfigurationExpression(string paramName, TypeMapConfiguration config)
    {
        _paramName = paramName;
        _config = config;
    }

    public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
    {
        var memberName = GetMemberName(sourceMember);
        _config.CtorParamMappings[_paramName] = memberName;
    }

    private static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (expression.Body is UnaryExpression { Operand: MemberExpression unaryMember })
            return unaryMember.Member.Name;

        throw new ArgumentException($"Expression '{expression}' does not refer to a member.", nameof(expression));
    }
}
