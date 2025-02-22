using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Snowflake.Tests;

public sealed class GeneratorTests
{
    [Fact]
    public void Generate_ShouldNotGenerateCollisions()
    {
        var generator = new IdentifierGenerator(
            new IdentifierGeneratorConfiguration(DateTimeOffset.UtcNow.AddDays(-1)),
            datacenterId: 1,
            machineId: 1);

        const int parallelism = 25;
        const int numberOfIdentifiers = 10_000_000;

        var identifierStorage = new ConcurrentDictionary<long, byte>();
        Parallel.For(0, numberOfIdentifiers, new ParallelOptions
        {
            MaxDegreeOfParallelism = parallelism
        }, _ =>
        {
            identifierStorage.TryAdd(generator.Generate(), 1);
        });

        identifierStorage.Count.Should().Be(numberOfIdentifiers);
    }
}