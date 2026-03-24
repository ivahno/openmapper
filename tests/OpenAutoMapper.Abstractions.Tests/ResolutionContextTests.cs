using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class ResolutionContextTests
{
    [Fact]
    public void Constructor_SetsMapperProperty()
    {
        var mapper = new StubMapper();

        var context = new ResolutionContext(mapper);

        context.Mapper.Should().BeSameAs(mapper);
    }

    [Fact]
    public void Constructor_ThrowsOnNullMapper()
    {
        var act = () => new ResolutionContext(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Items_IsInitializedAsEmptyDictionary()
    {
        var mapper = new StubMapper();

        var context = new ResolutionContext(mapper);

        context.Items.Should().NotBeNull();
        context.Items.Should().BeEmpty();
    }

    [Fact]
    public void Items_CanAddAndRetrieveValues()
    {
        var mapper = new StubMapper();
        var context = new ResolutionContext(mapper);

        context.Items["key1"] = "value1";
        context.Items["key2"] = 42;

        context.Items["key1"].Should().Be("value1");
        context.Items["key2"].Should().Be(42);
        context.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Minimal IMapper stub that throws NotImplementedException for all methods.
    /// Used only to satisfy the ResolutionContext constructor requirement.
    /// </summary>
    private sealed class StubMapper : IMapper
    {
        public TDestination Map<TDestination>(object source) =>
            throw new NotImplementedException();

        public TDestination Map<TSource, TDestination>(TSource source) =>
            throw new NotImplementedException();

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
            throw new NotImplementedException();

        public object Map(object source, Type sourceType, Type destinationType) =>
            throw new NotImplementedException();

        public object Map(object source, object destination, Type sourceType, Type destinationType) =>
            throw new NotImplementedException();

        public TDestination Map<TSource, TDestination>(TSource source, string mappingName) =>
            throw new NotImplementedException();
    }
}
