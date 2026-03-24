#nullable enable

using System;
using System.Collections.Generic;

namespace OpenAutoMapper.Exceptions;

/// <summary>
/// Exception thrown when mapper configuration validation fails.
/// </summary>
public class AutoMapperConfigurationException : OpenAutoMapperException
{
    public AutoMapperConfigurationException()
    {
    }

    public AutoMapperConfigurationException(string message)
        : base(message)
    {
    }

    public AutoMapperConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public AutoMapperConfigurationException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    /// The individual validation errors that caused this exception.
    /// </summary>
    public IEnumerable<string>? Errors { get; }
}
