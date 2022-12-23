using System;
using System.Data.HashFunction.xxHash;
using System.Text;

namespace Snowflake.Runner;

public sealed class XxHashingFunction
{
    private readonly IxxHash _hash;

    public XxHashingFunction()
    {
        _hash = xxHashFactory.Instance.Create(
            new xxHashConfig
            {
                HashSizeInBits = 32,
            });
    }

    public int Calculate(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key should not be empty or null", nameof(key));
        }

        var bytesHash = _hash.ComputeHash(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToInt32(bytesHash.Hash);
    }
}