using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class TypePairTests
{
    [Fact]
    public void Constructor_SetsSourceAndDestinationType()
    {
        var pair = new TypePair(typeof(string), typeof(int));

        pair.SourceType.Should().Be(typeof(string));
        pair.DestinationType.Should().Be(typeof(int));
    }

    [Fact]
    public void Constructor_ThrowsOnNullSourceType()
    {
        var act = () => new TypePair(null!, typeof(int));
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sourceType");
    }

    [Fact]
    public void Constructor_ThrowsOnNullDestinationType()
    {
        var act = () => new TypePair(typeof(string), null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("destinationType");
    }

    [Fact]
    public void Equals_SameTypes_ReturnsTrue()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(string), typeof(int));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentSourceType_ReturnsFalse()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(double), typeof(int));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentDestType_ReturnsFalse()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(string), typeof(double));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_ReturnsTrue_ForSameTypePair()
    {
        var a = new TypePair(typeof(string), typeof(int));
        object b = new TypePair(typeof(string), typeof(int));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_Object_ReturnsFalse_ForNonTypePair()
    {
        var a = new TypePair(typeof(string), typeof(int));

        a.Equals("not a type pair").Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_ReturnsFalse_ForNull()
    {
        var a = new TypePair(typeof(string), typeof(int));

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameTypes_SameHash()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(string), typeof(int));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentTypes_DifferentHash()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(int), typeof(string));

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_ReturnsTrue_ForEqual()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(string), typeof(int));

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ReturnsTrue_ForDifferent()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(int), typeof(string));

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ReturnsFalse_ForEqual()
    {
        var a = new TypePair(typeof(string), typeof(int));
        var b = new TypePair(typeof(string), typeof(int));

        (a != b).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var pair = new TypePair(typeof(string), typeof(int));

        pair.ToString().Should().Be("String -> Int32");
    }
}
