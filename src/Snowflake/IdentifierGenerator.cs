using System;
using System.Threading;

namespace Snowflake;

public sealed class IdentifierGenerator
{
    private readonly IdentifierGeneratorConfiguration _configuration;
    private SpinWait _spin = default;

    private readonly uint _datacenterShift;
    private readonly uint _podShift;
    private readonly uint _maxSequenceNumber;

    private volatile uint _currentSequence = 0;
    private long _latestSequenceTimeUsage;

    public IdentifierGenerator(IdentifierGeneratorConfiguration configuration, int datacenterId, int podId)
    {
        _configuration = configuration;

        _datacenterShift = GetFirstBits(datacenterId, bits: _configuration.DatacenterBits);
        _podShift = GetFirstBits(podId, bits: _configuration.MachineBits);
        _maxSequenceNumber = (uint)(Math.Pow(2, _configuration.SequenceBits) - 1);
    }

    public long Generate()
    {
        var currentMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (currentMilliseconds == _latestSequenceTimeUsage)
        {
            Interlocked.Increment(ref _currentSequence);
            Interlocked.And(ref _currentSequence, _maxSequenceNumber);

            // overflow
            if (_currentSequence == 0)
            {
                currentMilliseconds = WaitTillNextMillisecond(currentMilliseconds);
            }
        }
        else
        {
            Interlocked.Exchange(ref _currentSequence, 0);
        }

        Interlocked.Exchange(ref _latestSequenceTimeUsage, currentMilliseconds);

        var result = currentMilliseconds - _configuration.StartEpoch;
        result <<= _configuration.SequenceBits + _configuration.MachineBits + _configuration.DatacenterBits;
        result |= _datacenterShift << _configuration.SequenceBits + _configuration.MachineBits;
        result |= _podShift << _configuration.SequenceBits;
        result |= _currentSequence;

        return result;
    }

    private long WaitTillNextMillisecond(long startMilliseconds)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (startMilliseconds == currentTime)
        {
            // Getting rid of busy wait
            _spin.SpinOnce(sleep1Threshold: 1);
            currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        return currentTime;
    }

    private static uint GetFirstBits(int value, int bits)
    {
        if (bits >= 32)
        {
            throw new InvalidOperationException("Cannot shift an integer value for more than 31 bits");
        }

        var mask = (1 << bits) - 1;
        return (uint)(value & mask);
    }
}