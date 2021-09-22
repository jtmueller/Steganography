namespace Steganography;

using System.Collections;
using System.Runtime.CompilerServices;

// borrowed from https://codereview.stackexchange.com/questions/104736/sieve32fastv2-a-fast-parallel-sieve-of-eratosthenes
public class Sieve32
{
    private static ArgumentException BadUpperLimitException(string paramName) => new($"{paramName} be must greater than or equal to 2.");

    public static IEnumerable<uint> Primes(int upperLimit)
    {
        if (upperLimit < 2)
        {
            throw BadUpperLimitException(nameof(upperLimit));
        }

        return Primes((uint)upperLimit);
    }

    public static IEnumerable<uint> Primes(uint upperLimit)
    {
        if (upperLimit < 2)
        {
            throw BadUpperLimitException(nameof(upperLimit));
        }

        var instance = new Sieve32(upperLimit);
        return instance.EnumeratePrimes();
    }

    private Sieve32(uint upperLimit)
    {
        _vectors = VectorList.Create(upperLimit);
    }

    private readonly VectorList _vectors;

    // Performance Tweak:
    //    Favor "(number & 0x01U) == 0U" over "number % 2 == 0" or "number % 2U == 0U"
    //    Favor "x >> 1" over "x / 2"
    //    Favor "x << 1" over "x * 2" or "x + x"
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEven(uint number) => (number & 0x01U) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEven(int index) => (index & 0x01) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Halved(int value) => value >> 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Halved(uint value) => value >> 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Doubled(int value) => value << 1;

    private IEnumerable<uint> EnumeratePrimes()
    {
        if (_vectors.UpperLimit < 2) yield break;

        yield return 2;
        if (_vectors.UpperLimit == 2) yield break;

        // I call _vectors[0] the rootVector not just because its the very first one, but also because
        // it was intentionally created so that the index of upperLimit's square root is contained within _vectors[0].
        var rootVector = _vectors[0];

        // Performance Tweak:  Create copy of value in local variable
        int rootBitIndex = _vectors.SquareRootIndex;

        // The number of times a bit in all BitArray(s) are accessed:
        //
        //      UpperLimit = int.MaxValue    =>   3,315,151,693 times
        //      UpperLimit = uint.MaxValue   =>   6,701,709,402 times

        for (int bitIndex = 0; bitIndex <= rootBitIndex; bitIndex++)
        {
            if (rootVector[bitIndex])
            {
                uint prime = rootVector.ToNumber(bitIndex);
                yield return prime;

                // All multiples of prime - on all vectors - are composites and should be marked as such.
                MarkCompositesInParallel(prime);
            }
        }

        // Output remaining primes found beyond rootBitIndex

        // Performance Tweak:  Favor "for" over "foreach"
        for (int vectorIndex = 0; vectorIndex < _vectors.Count; vectorIndex++)
        {
            var vector = _vectors[vectorIndex];

            // Performance Tweak:  Favor "<= stopIndex" over "< vector.BitLength".  Calc stopIndex once before loop.
            int stopIndex = vector.BitLength - 1;

            // Not a performance tweak: startIndex is a separate variable just for readability.
            int startIndex = (vectorIndex == 0) ? rootBitIndex + 1 : 0;

            for (int bitIndex = startIndex; bitIndex <= stopIndex; bitIndex++)
            {
                if (vector[bitIndex]) { yield return vector.ToNumber(bitIndex); }
            }
        }
    }

    private void MarkCompositesInParallel(uint prime)
    {
        // Performance Tweak:  Favor "Parallel.For" over "Parallel.ForEach"
        Parallel.For(0, _vectors.Count, vectorIndex =>
        {
            var vector = _vectors[vectorIndex];

            // Performance Tweak:  Create copy local to lambda.  Cast once to bitStep before the inner loop.
            int bitStep = (int)prime;

            int startIndex = 0;

            if (vectorIndex == 0)
            {
                startIndex = vector.ToIndex(prime * prime);
            }
            else
            {
                int remainder = (int)(vector.StartingNumber % prime);

                // If remainder is 0, then we will use startIndex's initial value of 0.

                if (remainder != 0)
                {
                    startIndex = bitStep - remainder;

                    // On the full number scale, every other multiple of prime is even and should be skipped 
                    // over for the next multiple, which is an odd number. 
                    if (IsEven(remainder))
                    {
                        startIndex += bitStep;
                    }

                    startIndex = Halved(startIndex);
                }
            }

            // Performance Tweak:  Favor "<= stopIndex" over "< vector.BitLength".  Calc stopIndex once before loop.
            int stopIndex = vector.BitLength - 1;

            for (int bitIndex = startIndex; bitIndex <= stopIndex; bitIndex += bitStep)
            {
                vector[bitIndex] = false;
            }
        });
    }

    private class VectorList : List<Vector>
    {
        // May not slap you in the face, but these 2 properties are readonly.
        public uint UpperLimit { get; }
        public int SquareRootIndex { get; }

        public static VectorList Create(uint upperLimit)
        {
            var instance = new VectorList(upperLimit);
            instance.CreateVectors();
            return instance;
        }

        private VectorList(uint upperLimit)
        {
            // Any upperLimit > 2 should be odd for working with VectorList and Vector(s).
            if ((upperLimit > 2) && IsEven(upperLimit))
            {
                upperLimit--;
            }

            UpperLimit = upperLimit;
            SquareRootIndex = ToFlatIndex((uint)Math.Sqrt(upperLimit));
        }

        private void CreateVectors()
        {
            int typicalBitLength = CalcTypicalBitLength();
            uint typicalNumberRange = (uint)Doubled(typicalBitLength);
            uint count = (UpperLimit / typicalNumberRange) + 1;

            for (uint i = 0, endingNumber = 1; i <= count; i++)
            {
                if (endingNumber >= UpperLimit) { break; }

                // The first vector may have to be longer to accomodate the index of UpperLimit's square root.
                int length = (i == 0) ? GetSpecialFirstLength(typicalBitLength) : typicalBitLength;
                uint startingNumber = endingNumber + 2;

                var vector = new Vector(startingNumber, length, UpperLimit);
                Add(vector);

                endingNumber = vector.EndingNumber;
            }
        }

        private int CalcTypicalBitLength()
        {
            // This is called before we have created a _vectors[0].
            int length = ToFlatIndex(UpperLimit) + 1;

            // Small enough values will result in 1 vector
            const uint smallNumberCutoff = 10000;
            if (UpperLimit < smallNumberCutoff) { return length; }

            // Divide length for later parallelization over many (but not too many) vectors.
            const int tinyFactor = 8;
            int maxVectorCount = tinyFactor * Environment.ProcessorCount;
            length = (length / maxVectorCount) + 1;

            return PaddedLength(length);
        }

        private int GetSpecialFirstLength(int length) => (SquareRootIndex < length) ? length : PaddedLength(SquareRootIndex + 1);

        private static int ToFlatIndex(uint number) => (int)Halved(number - 3);

        private static int PaddedLength(int length)
        {
            // BitArray internally uses 32 bit int[] so align upwards to a 32 bit boundary, 
            // i.e. pad the end of length (in bits) to consume a full 32 bit int.
            int remainder = length % 32;
            return (remainder == 0) ? length : length + 32 - remainder;
        }
    }

    // A Vector is aware of its bits, length, starting number, and ending number.
    private class Vector
    {
        private readonly BitArray _bits;

        public Vector(uint startNumber, int length, uint upperLimit)
        {
            StartingNumber = startNumber;
            long endNumber = startNumber + Doubled(length - 1);

            // In this constructor, endNumber is a long that could be > uint.MaxValue,
            // in which case we clamp the length appropriately.
            if (endNumber > upperLimit)
            {
                length = ToIndex(upperLimit) + 1;
            }

            _bits = new BitArray(length, defaultValue: true);
        }

        public bool this[int index] { get => _bits[index]; set => _bits[index] = value; }

        public int BitLength => _bits.Length;

        // May not slap you in the face, but this property is readonly.
        public uint StartingNumber { get; }

        public uint EndingNumber => ToNumber(_bits.Length - 1);

        public int ToIndex(uint number) => (int)Halved(number - StartingNumber);

        public uint ToNumber(int bitIndex) => (uint)Doubled(bitIndex) + StartingNumber;
    }
}
