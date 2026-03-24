using System;
using System.Linq;
using FluentAssertions;
using OpenAutoMapper;
using OpenAutoMapper.Exceptions;
using Xunit;

namespace OpenAutoMapper.Core.Tests;

public class ExceptionTests
{
    // --- OpenAutoMapperException ---

    [Fact]
    public void OpenAutoMapperException_InheritsFromException()
    {
        var ex = new OpenAutoMapperException();

        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void OpenAutoMapperException_DefaultConstructor_CreatesInstance()
    {
        var ex = new OpenAutoMapperException();

        ex.Should().NotBeNull();
        ex.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OpenAutoMapperException_MessageConstructor_SetsMessage()
    {
        var ex = new OpenAutoMapperException("test error");

        ex.Message.Should().Be("test error");
    }

    [Fact]
    public void OpenAutoMapperException_MessageAndInnerConstructor_SetsBoth()
    {
        var inner = new InvalidOperationException("inner error");

        var ex = new OpenAutoMapperException("outer error", inner);

        ex.Message.Should().Be("outer error");
        ex.InnerException.Should().BeSameAs(inner);
    }

    // --- AutoMapperMappingException ---

    [Fact]
    public void AutoMapperMappingException_InheritsFromOpenAutoMapperException()
    {
        var ex = new AutoMapperMappingException();

        ex.Should().BeAssignableTo<OpenAutoMapperException>();
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void AutoMapperMappingException_DefaultConstructor_CreatesInstance()
    {
        var ex = new AutoMapperMappingException();

        ex.Should().NotBeNull();
        ex.TypePair.Should().BeNull();
    }

    [Fact]
    public void AutoMapperMappingException_MessageConstructor_SetsMessage()
    {
        var ex = new AutoMapperMappingException("mapping failed");

        ex.Message.Should().Be("mapping failed");
        ex.TypePair.Should().BeNull();
    }

    [Fact]
    public void AutoMapperMappingException_MessageAndInnerConstructor_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new AutoMapperMappingException("mapping failed", inner);

        ex.Message.Should().Be("mapping failed");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AutoMapperMappingException_MessageAndTypePairConstructor_SetsBoth()
    {
        var typePair = new TypePair(typeof(string), typeof(int));

        var ex = new AutoMapperMappingException("mapping failed", typePair);

        ex.Message.Should().Be("mapping failed");
        ex.TypePair.Should().Be(typePair);
    }

    [Fact]
    public void AutoMapperMappingException_FullConstructor_SetsAll()
    {
        var inner = new InvalidOperationException("inner");
        var typePair = new TypePair(typeof(string), typeof(int));

        var ex = new AutoMapperMappingException("mapping failed", inner, typePair);

        ex.Message.Should().Be("mapping failed");
        ex.InnerException.Should().BeSameAs(inner);
        ex.TypePair.Should().Be(typePair);
    }

    // --- AutoMapperConfigurationException ---

    [Fact]
    public void AutoMapperConfigurationException_InheritsFromOpenAutoMapperException()
    {
        var ex = new AutoMapperConfigurationException();

        ex.Should().BeAssignableTo<OpenAutoMapperException>();
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void AutoMapperConfigurationException_DefaultConstructor_CreatesInstance()
    {
        var ex = new AutoMapperConfigurationException();

        ex.Should().NotBeNull();
        ex.Errors.Should().BeNull();
    }

    [Fact]
    public void AutoMapperConfigurationException_MessageConstructor_SetsMessage()
    {
        var ex = new AutoMapperConfigurationException("config error");

        ex.Message.Should().Be("config error");
        ex.Errors.Should().BeNull();
    }

    [Fact]
    public void AutoMapperConfigurationException_MessageAndInnerConstructor_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new AutoMapperConfigurationException("config error", inner);

        ex.Message.Should().Be("config error");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AutoMapperConfigurationException_MessageAndErrorsConstructor_SetsBoth()
    {
        var errors = new[] { "Error 1", "Error 2" };

        var ex = new AutoMapperConfigurationException("config error", errors);

        ex.Message.Should().Be("config error");
        ex.Errors.Should().NotBeNull();
        ex.Errors!.ToList().Should().HaveCount(2);
        ex.Errors!.Should().Contain("Error 1");
        ex.Errors!.Should().Contain("Error 2");
    }

    // --- Inheritance chain verification ---

    [Fact]
    public void InheritanceChain_AutoMapperMappingException_Extends_OpenAutoMapperException_Extends_Exception()
    {
        typeof(AutoMapperMappingException).BaseType.Should().Be(typeof(OpenAutoMapperException));
        typeof(OpenAutoMapperException).BaseType.Should().Be(typeof(Exception));
    }

    [Fact]
    public void InheritanceChain_AutoMapperConfigurationException_Extends_OpenAutoMapperException_Extends_Exception()
    {
        typeof(AutoMapperConfigurationException).BaseType.Should().Be(typeof(OpenAutoMapperException));
        typeof(OpenAutoMapperException).BaseType.Should().Be(typeof(Exception));
    }
}
