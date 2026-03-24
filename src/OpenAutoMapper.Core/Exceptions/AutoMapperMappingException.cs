#nullable enable

using System;

namespace OpenAutoMapper.Exceptions;

/// <summary>
/// Exception thrown when a mapping operation fails at runtime.
/// Named for AutoMapper compatibility.
/// </summary>
public class AutoMapperMappingException : OpenAutoMapperException
{
    public AutoMapperMappingException()
    {
    }

    public AutoMapperMappingException(string message)
        : base(message)
    {
    }

    public AutoMapperMappingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public AutoMapperMappingException(string message, TypePair typePair)
        : base(message)
    {
        TypePair = typePair;
    }

    public AutoMapperMappingException(string message, Exception innerException, TypePair typePair)
        : base(message, innerException)
    {
        TypePair = typePair;
    }

    /// <summary>
    /// The source-destination type pair that caused the mapping failure, if available.
    /// </summary>
    public TypePair? TypePair { get; }
}
