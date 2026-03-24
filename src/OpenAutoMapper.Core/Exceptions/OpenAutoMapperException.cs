#nullable enable

using System;

namespace OpenAutoMapper.Exceptions;

/// <summary>
/// Base exception for all OpenAutoMapper errors.
/// </summary>
public class OpenAutoMapperException : Exception
{
    public OpenAutoMapperException()
    {
    }

    public OpenAutoMapperException(string message)
        : base(message)
    {
    }

    public OpenAutoMapperException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
