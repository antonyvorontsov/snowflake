using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Snowflake;
using Snowflake.Runner;

var hash = new XxHashingFunction();

var startEpoch = new DateTimeOffset(
    new DateTime(
        2022,
        01,
        01,
        00,
        00,
        00));
var generatorConfiguration = new IdentifierGeneratorConfiguration(startEpoch);

var generators = new[]
{
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-1"),
        hash.Calculate("service-pod-1")),
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-1"),
        hash.Calculate("service-pod-2")),

    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-2"),
        hash.Calculate("service-pod-3")),
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-2"),
        hash.Calculate("service-pod-4")),

    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-3"),
        hash.Calculate("service-pod-5")),
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-3"),
        hash.Calculate("service-pod-6")),

    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-4"),
        hash.Calculate("service-pod-7")),
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-4"),
        hash.Calculate("service-pod-8")),

    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-5"),
        hash.Calculate("service-pod-9")),
    new IdentifierGenerator(
        generatorConfiguration,
        hash.Calculate("datacenter-5"),
        hash.Calculate("service-pod-10")),
};

var bag = new ConcurrentBag<long>();

ParallelOptions parallelOptions = new()
{
    MaxDegreeOfParallelism = generators.Length
};

var stopwatch = new Stopwatch();
stopwatch.Start();

Parallel.ForEach(generators, parallelOptions, generator =>
{
    for (var i = 0; i < 1_000_000; i++)
    {
        var id = generator.Generate();
        bag.Add(id);
    }

    Console.WriteLine("finished");
});

stopwatch.Stop();

Console.WriteLine();
Console.WriteLine($"elapsed time in ms: {stopwatch.ElapsedMilliseconds}");

var collisions = bag.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count())
    .Where(x => x.Value > 1)
    .ToArray();

if (collisions.Any())
{
    foreach (var collision in collisions)
    {
        Console.WriteLine($"{collision.Key}: {collision.Value}");
    }
}
else
{
    // And we are not expecting those
    Console.WriteLine("no collisions");
}
