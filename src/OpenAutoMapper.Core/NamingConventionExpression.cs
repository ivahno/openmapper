#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Internal implementation of <see cref="INamingConventionExpression"/>.
/// </summary>
internal sealed class NamingConventionExpression : INamingConventionExpression
{
    public INamingConvention? SourceMemberNamingConvention { get; set; }

    public INamingConvention? DestinationMemberNamingConvention { get; set; }
}
