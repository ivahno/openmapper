using System;
using FluentAssertions;
using OpenAutoMapper;
using Xunit;

namespace OpenAutoMapper.Abstractions.Tests;

public class ProfileTests
{
    [Fact]
    public void RecognizePrefixes_DoesNotThrow()
    {
        var act = () => new PrefixTestProfile();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecognizePostfixes_DoesNotThrow()
    {
        var act = () => new PostfixTestProfile();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddGlobalIgnore_DoesNotThrow()
    {
        var act = () => new GlobalIgnoreTestProfile();
        act.Should().NotThrow();
    }

    [Fact]
    public void EmptyProfile_CanBeConstructed()
    {
        var profile = new EmptyProfile();
        profile.Should().NotBeNull();
        profile.Should().BeAssignableTo<Profile>();
    }

    [Fact]
    public void MultiplePrefixCalls_DoNotThrow()
    {
        var act = () => new MultiplePrefixProfile();
        act.Should().NotThrow();
    }

    // --- Helper profiles ---

    private sealed class EmptyProfile : Profile { }

    private sealed class PrefixTestProfile : Profile
    {
        public PrefixTestProfile() { RecognizePrefixes("Get", "Is"); }
    }

    private sealed class PostfixTestProfile : Profile
    {
        public PostfixTestProfile() { RecognizePostfixes("Dto"); }
    }

    private sealed class GlobalIgnoreTestProfile : Profile
    {
        public GlobalIgnoreTestProfile() { AddGlobalIgnore("_internal"); }
    }

    private sealed class MultiplePrefixProfile : Profile
    {
        public MultiplePrefixProfile()
        {
            RecognizePrefixes("Get");
            RecognizePrefixes("Is");
            RecognizePostfixes("Dto", "Model");
        }
    }
}
