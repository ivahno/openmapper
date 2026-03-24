using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class MemberListTests
{
    [Fact]
    public void MemberList_Destination_HasValue0()
    {
        ((int)MemberList.Destination).Should().Be(0);
    }

    [Fact]
    public void MemberList_Source_HasValue1()
    {
        ((int)MemberList.Source).Should().Be(1);
    }

    [Fact]
    public void MemberList_None_HasValue2()
    {
        ((int)MemberList.None).Should().Be(2);
    }
}
