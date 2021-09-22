namespace Steganography;

using System;
using System.Collections.Generic;

internal static class Extensions
{
    public static T RandomOrDefault<T>(this IEnumerable<T> source, Random rng, T defaultValue = default!)
    {
        T current = defaultValue;
        int count = 0;
        foreach (T element in source)
        {
            count++;
            if (rng.Next(count) == 0)
            {
                current = element;
            }
        }
        if (count == 0)
        {
            throw new InvalidOperationException("Sequence was empty");
        }
        return current;
    }
}
