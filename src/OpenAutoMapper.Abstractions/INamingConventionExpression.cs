#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Expression for configuring source and destination naming conventions.
/// </summary>
public interface INamingConventionExpression
{
    INamingConvention? SourceMemberNamingConvention { get; set; }

    INamingConvention? DestinationMemberNamingConvention { get; set; }
}
