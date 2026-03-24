#nullable enable

using System.Text.RegularExpressions;

namespace OpenAutoMapper;

/// <summary>
/// Defines a naming convention used to split and match member names.
/// </summary>
public interface INamingConvention
{
    string SeparatorCharacter { get; }

    Regex SplittingExpression { get; }
}
