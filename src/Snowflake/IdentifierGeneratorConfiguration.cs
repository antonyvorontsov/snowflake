using System;

namespace Snowflake;

public sealed class IdentifierGeneratorConfiguration
{
    public ushort EpochBits { get; }
    public ushort DatacenterBits { get; }
    public ushort MachineBits { get; }
    public ushort SequenceBits { get; }
    public long StartEpoch { get; }

    public IdentifierGeneratorConfiguration(
        ushort epochBits,
        ushort datacenterBits,
        ushort machineBits,
        ushort sequenceBits,
        DateTimeOffset startEpoch)
    {
        if (epochBits + datacenterBits + machineBits + sequenceBits != 63)
        {
            throw new ArgumentException(
                "The sum of the bits has to be 63 (plus first bit is leading)");
        }

        if (startEpoch > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Start epoch has to be in the past");
        }

        EpochBits = epochBits;
        DatacenterBits = datacenterBits;
        MachineBits = machineBits;
        SequenceBits = sequenceBits;
        StartEpoch = startEpoch.ToUnixTimeMilliseconds();
    }

    public IdentifierGeneratorConfiguration(DateTimeOffset startEpoch)
        : this(
            epochBits: Defaults.EpochBits,
            datacenterBits: Defaults.DatacenterBits,
            machineBits: Defaults.MachineBits,
            sequenceBits: Defaults.SequenceBits,
            startEpoch)
    {
    }
}