using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Snowflake.Tests;

public sealed class IdentifierGeneratorTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 3000)]
    [InlineData(1, 10_000)]
    [InlineData(2, 20_000)]
    [InlineData(15, 1_000_000)]
    [InlineData(25, 2_000_000)]
    public void Generates_identifiers_with_no_collisions(int parallelism, int numberOfIdentifiers)
    {
        // Arrange
        var identifierStorage = new ConcurrentDictionary<long, byte>();
        var generator = new IdentifierGenerator(
            new IdentifierGeneratorConfiguration(DateTimeOffset.UtcNow.AddDays(-1)),
            datacenterId: 1,
            machineId: 1);

        // Act
        Parallel.For(0, numberOfIdentifiers, new ParallelOptions
        {
            MaxDegreeOfParallelism = parallelism
        }, _ => { identifierStorage.TryAdd(generator.Generate(), 1); });

        // Assert
        identifierStorage.Count.Should().Be(numberOfIdentifiers);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(10_000)]
    [InlineData(1_000_000)]
    public void Generates_monotonically_increasing_identifiers(int numberOfIdentifiers)
    {
        // Arrange
        var identifierStorage = new List<long>();
        var generator = new IdentifierGenerator(
            new IdentifierGeneratorConfiguration(DateTimeOffset.UtcNow.AddDays(-1)),
            datacenterId: 1,
            machineId: 1);

        // Act
        for (var i = 0; i < numberOfIdentifiers; i++)
        {
            identifierStorage.Add(generator.Generate());
        }

        Thread.Sleep(10); // acts as a pause between iterations

        for (var i = 0; i < numberOfIdentifiers; i++)
        {
            identifierStorage.Add(generator.Generate());
        }

        // Assert
        for (var i = 1; i < identifierStorage.Count; i++)
        {
            identifierStorage[i].Should().BeGreaterThan(identifierStorage[i - 1]);
        }
    }
}