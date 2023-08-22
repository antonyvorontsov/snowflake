namespace Snowflake;

// You can define your own defaults obviously.
// Sometimes you don't need to have the "datacenter bits" part, thus you can remove it and add the rest of the bits
// to the "machine" (pod) part.
public static class Defaults
{
    public const int EpochBits = 41;
    public const int DatacenterBits = 5;
    public const int MachineBits = 5;
    public const int SequenceBits = 12;
}