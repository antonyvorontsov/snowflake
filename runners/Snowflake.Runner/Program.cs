using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Snowflake;

var startEpoch = new DateTimeOffset(
    new DateTime(
        2020,
        01,
        01,
        00,
        00,
        00));
var generatorConfiguration = new IdentifierGeneratorConfiguration(startEpoch);

var datacenters = new Dictionary<string, int>
{
    ["datacenter-1"] = 1,
    ["datacenter-2"] = 2,
    ["datacenter-3"] = 3,
    ["datacenter-4"] = 4,
    ["datacenter-5"] = 5
};

var pods = new Dictionary<string, int>
{
    ["service-pod-1"] = 1,
    ["service-pod-2"] = 2,
    ["service-pod-3"] = 3,
    ["service-pod-4"] = 4,
    ["service-pod-5"] = 5,
    ["service-pod-6"] = 6,
    ["service-pod-7"] = 7,
    ["service-pod-8"] = 8,
    ["service-pod-9"] = 9,
    ["service-pod-10"] = 10
};

var generators = new[]
{
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-1"],
        pods["service-pod-1"]),
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-1"],
        pods["service-pod-2"]),

    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-2"],
        pods["service-pod-3"]),
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-2"],
        pods["service-pod-4"]),

    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-3"],
        pods["service-pod-5"]),
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-3"],
        pods["service-pod-6"]),

    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-4"],
        pods["service-pod-7"]),
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-4"],
        pods["service-pod-8"]),

    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-5"],
        pods["service-pod-9"]),
    new IdentifierGenerator(
        generatorConfiguration,
        datacenters["datacenter-5"],
        pods["service-pod-10"])
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
