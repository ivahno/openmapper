#nullable enable

namespace OpenAutoMapper.Internal;

/// <summary>
/// Represents a path-based mapping (ForPath) with a dotted destination path and source expression.
/// </summary>
internal sealed class PathMap
{
    public PathMap(string destinationPath)
    {
        DestinationPath = destinationPath;
    }

    /// <summary>
    /// The dotted path to the destination property (e.g., "Address.City").
    /// </summary>
    public string DestinationPath { get; }

    /// <summary>
    /// The source expression used to produce the value for this path.
    /// </summary>
    public object? SourceExpression { get; set; }
}
