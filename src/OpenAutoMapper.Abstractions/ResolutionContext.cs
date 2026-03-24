#nullable enable

using System;
using System.Collections.Generic;

namespace OpenAutoMapper;

/// <summary>
/// Context information passed to resolvers, converters, and actions during mapping.
/// </summary>
public class ResolutionContext
{
    public ResolutionContext(IMapper mapper)
    {
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        Items = new Dictionary<string, object>();
    }

    private ResolutionContext(IMapper mapper, IDictionary<string, object> items)
    {
        Mapper = mapper;
        Items = items;
    }

    /// <summary>
    /// The mapper instance performing the current mapping operation.
    /// </summary>
    public IMapper Mapper { get; }

    /// <summary>
    /// Contextual items that can be passed through the mapping pipeline.
    /// </summary>
    public IDictionary<string, object> Items { get; }

    /// <summary>
    /// Internal factory for creating resolution contexts (supports future pooling).
    /// </summary>
    internal static ResolutionContext Create(IMapper mapper, IDictionary<string, object>? items = null)
    {
        return new ResolutionContext(mapper, items ?? new Dictionary<string, object>());
    }
}
