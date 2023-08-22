using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Snowflake.Tests;

public sealed class GeneratorTests
{
    [Fact]
    public void Generate_MultipleParallelActions_ShouldNotGenerateCollisions()
    {
        var generator = new IdentifierGenerator(
            new IdentifierGeneratorConfiguration(DateTimeOffset.UtcNow.AddDays(-1)),
            datacenterId: 1,
            machineId: 1);

        const int parallelism = 15;
        const int numberOfIdentifiers = 1_000_000;

        var identifierStorage = Enumerable.Range(0, parallelism).Select(_ => new List<long>()).ToArray();
        var actions = identifierStorage.Select<List<long>, Action>(storage => () =>
        {
            for (var i = 0; i < numberOfIdentifiers; i++)
            {
                storage.Add(generator.Generate());
            }
        }).ToArray();
        Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = parallelism }, actions);

        var uniqueIdentifiers = identifierStorage.SelectMany(x => x).ToHashSet();
        uniqueIdentifiers.Count.Should().Be(parallelism * numberOfIdentifiers);
    }
    
    [Fact]
    public async Task Generate_MultipleTasks_ShouldNotGenerateCollisions()
    {
        var generator = new IdentifierGenerator(
            new IdentifierGeneratorConfiguration(DateTimeOffset.UtcNow.AddDays(-1)),
            datacenterId: 1,
            machineId: 1);

        const int parallelism = 15;
        const int numberOfIdentifiers = 1_000_000;

        var identifierStorage = Enumerable.Range(0, parallelism).Select(_ => new List<long>()).ToArray();

        Task GenerateIdentifiers(IdentifierGenerator snowflakeGenerator, List<long> storage)
        {
            for (var i = 0; i < numberOfIdentifiers; i++)
            {
                storage.Add(snowflakeGenerator.Generate());
            }

            return Task.CompletedTask;
        }

        await Task.WhenAll(Enumerable.Range(0, parallelism)
            .Select(x => GenerateIdentifiers(generator, identifierStorage[x])));

        var uniqueIdentifiers = identifierStorage.SelectMany(x => x).ToHashSet();
        uniqueIdentifiers.Count.Should().Be(parallelism * numberOfIdentifiers);
    }
}