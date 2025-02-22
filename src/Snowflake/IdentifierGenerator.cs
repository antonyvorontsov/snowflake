using System;
using System.Threading;

namespace Snowflake;

public sealed class IdentifierGenerator
{
    private readonly IdentifierGeneratorConfiguration _configuration;

    private readonly uint _datacenterShift;
    private readonly uint _podShift;
    private readonly uint _maxSequenceNumber;
    private readonly uint _sequenceTemplate;

    private uint _sequence;
    private long _lastUsage;

    public IdentifierGenerator(
        IdentifierGeneratorConfiguration configuration,
        int datacenterId,
        int machineId)
    {
        _configuration = configuration;
        _datacenterShift = GetFirstBits(datacenterId, bits: _configuration.DatacenterBits);
        _podShift = GetFirstBits(machineId, bits: _configuration.MachineBits);
        _maxSequenceNumber = (uint)Math.Pow(2, _configuration.SequenceBits);
        _sequenceTemplate = _maxSequenceNumber - 1;
    }

    public long Generate()
    {
        var currentMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sequence = NextSequence(currentMilliseconds);
        while (sequence >= _maxSequenceNumber)
        {
            currentMilliseconds = WaitTillNextMillisecond(currentMilliseconds);
            sequence = NextSequence(currentMilliseconds);
        }

        return AssembleIdentifier(currentMilliseconds, sequence);
    }

    private uint NextSequence(long callMilliseconds)
    {
        while (true)
        {
            var localLastUsage = Interlocked.Read(ref _lastUsage);
            var localSequence = _sequence;
            // Кто-то успел обновить значение быстрее нас
            if (localLastUsage > callMilliseconds)
            {
                return _maxSequenceNumber;
            }

            uint nextSequenceValue = 0;
            if (localLastUsage == callMilliseconds)
            {
                // Если получили переполнение, то выходим - исчерпали лимит в sequence.
                nextSequenceValue = (localSequence + 1) & _sequenceTemplate;
                if (nextSequenceValue == 0)
                {
                    return _maxSequenceNumber;
                }
            }

            if (Interlocked.CompareExchange(ref _lastUsage, callMilliseconds, localLastUsage) == localLastUsage
                && Interlocked.CompareExchange(ref _sequence, nextSequenceValue, localSequence) == localSequence)
            {
                return nextSequenceValue;
            }
        }
    }

    private long AssembleIdentifier(long currentMilliseconds, uint sequenceBits)
    {
        var elapsedMilliseconds = currentMilliseconds - _configuration.StartEpoch;

        var result = elapsedMilliseconds <<
                     (_configuration.SequenceBits + _configuration.MachineBits + _configuration.DatacenterBits);
        result |= _datacenterShift << (_configuration.SequenceBits + _configuration.MachineBits);
        result |= _podShift << _configuration.SequenceBits;
        result |= sequenceBits;

        return result;
    }

    private long WaitTillNextMillisecond(long startMilliseconds)
    {
        var spin = new SpinWait();
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (startMilliseconds == currentTime)
        {
            // Getting rid of busy wait
            spin.SpinOnce(sleep1Threshold: 1);
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