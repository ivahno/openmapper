using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Integration.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddAutoMapper_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddAutoMapper(typeof(DependencyInjectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var config = provider.GetService<IConfigurationProvider>();
        config.Should().NotBeNull();

        var mapperConfig = provider.GetService<MapperConfiguration>();
        mapperConfig.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenAutoMapper_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddOpenAutoMapper(typeof(DependencyInjectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var config = provider.GetService<IConfigurationProvider>();
        config.Should().NotBeNull();
    }

    [Fact]
    public void AddAutoMapper_WithConfigAction()
    {
        var services = new ServiceCollection();

        services.AddAutoMapper(
            cfg => cfg.CreateMap<SimpleSource, SimpleDest>(),
            typeof(DependencyInjectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var config = provider.GetService<MapperConfiguration>();
        config.Should().NotBeNull();
    }

    [Fact]
    public void AddAutoMapper_WithMarkerTypes()
    {
        var services = new ServiceCollection();

        services.AddAutoMapper(typeof(DependencyInjectionTests));

        var provider = services.BuildServiceProvider();
        var config = provider.GetService<IConfigurationProvider>();
        config.Should().NotBeNull();
    }

    [Fact]
    public void AddKeyedOpenAutoMapper_RegistersKeyedServices()
    {
        var services = new ServiceCollection();

        services.AddKeyedOpenAutoMapper("primary",
            cfg => cfg.CreateMap<SimpleSource, SimpleDest>());

        services.AddKeyedOpenAutoMapper("secondary",
            cfg => cfg.CreateMap<SimpleDest, SimpleSource>());

        var provider = services.BuildServiceProvider();

        var primaryConfig = provider.GetKeyedService<IConfigurationProvider>("primary");
        primaryConfig.Should().NotBeNull();

        var secondaryConfig = provider.GetKeyedService<IConfigurationProvider>("secondary");
        secondaryConfig.Should().NotBeNull();

        primaryConfig.Should().NotBeSameAs(secondaryConfig);
    }

    [Fact]
    public void AddKeyedOpenAutoMapper_ResolvesKeyedMapper()
    {
        var services = new ServiceCollection();

        services.AddKeyedOpenAutoMapper("orders",
            cfg => cfg.CreateMap<SimpleSource, SimpleDest>());

        var provider = services.BuildServiceProvider();

        var config = provider.GetKeyedService<MapperConfiguration>("orders");
        config.Should().NotBeNull();
    }

    public class SimpleSource
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class SimpleDest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
