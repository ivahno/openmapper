using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class InterfaceContractTests
{
    [Fact]
    public void IMapper_DefinesFiveMapOverloads()
    {
        var methods = typeof(IMapper).GetMethods();
        methods.Should().Contain(m => m.Name == "Map" && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 1);
        methods.Should().Contain(m => m.Name == "Map" && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 1);
        methods.Should().Contain(m => m.Name == "Map" && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 2);
        methods.Should().Contain(m => m.Name == "Map" && m.GetGenericArguments().Length == 0 && m.GetParameters().Length == 3);
        methods.Should().Contain(m => m.Name == "Map" && m.GetGenericArguments().Length == 0 && m.GetParameters().Length == 4);
    }

    [Fact]
    public void IValueResolver_HasResolveMethod()
    {
        var resolveMethod = typeof(IValueResolver<string, string, string>).GetMethod("Resolve");
        resolveMethod.Should().NotBeNull();
        resolveMethod!.GetParameters().Should().HaveCount(4);
    }

    [Fact]
    public void IMemberValueResolver_HasResolveMethod()
    {
        var resolveMethod = typeof(IMemberValueResolver<string, string, string, string>).GetMethod("Resolve");
        resolveMethod.Should().NotBeNull();
        resolveMethod!.GetParameters().Should().HaveCount(5);
    }

    [Fact]
    public void ITypeConverter_HasConvertMethod()
    {
        var method = typeof(ITypeConverter<string, int>).GetMethod("Convert");
        method.Should().NotBeNull();
        method!.GetParameters().Should().HaveCount(3);
    }

    [Fact]
    public void IValueConverter_HasConvertMethod()
    {
        var method = typeof(IValueConverter<string, int>).GetMethod("Convert");
        method.Should().NotBeNull();
        method!.GetParameters().Should().HaveCount(2);
    }

    [Fact]
    public void IMappingAction_HasProcessMethod()
    {
        var method = typeof(IMappingAction<string, int>).GetMethod("Process");
        method.Should().NotBeNull();
        method!.GetParameters().Should().HaveCount(3);
    }

    [Fact]
    public void IMappingExpression_HasForPathMethod()
    {
        typeof(IMappingExpression<string, string>).GetMethod("ForPath").Should().NotBeNull();
    }

    [Fact]
    public void IProjectionExpression_HasForPathMethod()
    {
        typeof(IProjectionExpression<string, string>).GetMethod("ForPath").Should().NotBeNull();
    }

    [Fact]
    public void IMemberConfigurationExpression_HasMapFromResolverOverloads()
    {
        var type = typeof(IMemberConfigurationExpression<string, string, string>);
        var methods = type.GetMethods();

        // Should have MapFrom with no params (generic resolver) and with expression param
        methods.Should().Contain(m => m.Name == "MapFrom" && m.GetGenericArguments().Length == 1);
        methods.Should().Contain(m => m.Name == "MapFrom" && m.GetGenericArguments().Length == 2);
    }

    private sealed class StubMapper : IMapper
    {
        public TDestination Map<TDestination>(object source) => throw new NotImplementedException();
        public TDestination Map<TSource, TDestination>(TSource source) => throw new NotImplementedException();
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => throw new NotImplementedException();
        public object Map(object source, Type sourceType, Type destinationType) => throw new NotImplementedException();
        public object Map(object source, object destination, Type sourceType, Type destinationType) => throw new NotImplementedException();
        public TDestination Map<TSource, TDestination>(TSource source, string mappingName) => throw new NotImplementedException();
    }
}
