using System;
using FluentAssertions;
using Xunit;

namespace Snowflake.Tests;

public sealed class IdentifierGeneratorConfigurationTests
{
    [Fact]
    public void Ctor_StartEpochInTheFuture_ThrowsException()
    {
        var function = () => new IdentifierGeneratorConfiguration(
            DateTimeOffset.UtcNow.AddDays(1));
        function.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_SumOfBitsDoesNotEqualTo63_ThrowsException()
    {
        var function = () => new IdentifierGeneratorConfiguration(
            10,
            10,
            10,
            10,
            DateTimeOffset.UtcNow.AddDays(-1));
        function.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_SumOfBitsEqualsTo63_DoesNotThrow()
    {
        var function = () => new IdentifierGeneratorConfiguration(
            41,
            5,
            5,
            12,
            DateTimeOffset.UtcNow.AddDays(-1));
        function.Should().NotThrow();
    }
}