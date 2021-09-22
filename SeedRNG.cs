namespace Steganography;

public class SeedRNG
{
    private readonly Random _rand;
    private readonly List<int> _exp = new();
    private readonly LCG _lcg;
    public SeedRNG(int seed, int limit, bool b = false)
    {
        _lcg = new LCG(limit, new Random(seed).Next(0, limit), seed);
        _rand = new Random(seed);
        if (b)
        {
            _exp = Enumerable.Range(0, limit).ToList();
        }
    }

    public int Next => (int)_lcg.Next();

    public int NextN
    {
        get
        {
            int n = _rand.Next(0, _exp.Count);
            int x = _exp[n];
            _exp.RemoveAt(n);
            return x;
        }
    }
}

public class SeedURNG
{
    private readonly List<uint> _obtained;
    private readonly uint _limit;
    private readonly Random _rand;
    private readonly List<uint> _exp = new();
    public SeedURNG(uint seed, uint limit, bool b = false)
    {
        _limit = limit;
        _obtained = new List<uint>();
        _rand = new Random((int)seed);
        if (b)
        {
            for (uint i = 0; i < limit; i++)
            {
                _exp.Add(i);
            }
        }
    }

    public uint Next
    {
        get
        {
            uint x;
            do
            {
                x = (uint)_rand.Next(0, (int)_limit);
            } while (_obtained.Contains(x));
            _obtained.Add(x);
            return x;
        }
    }

    public uint NextN
    {
        get
        {
            int n = _rand.Next(0, _exp.Count);
            uint x = _exp[n];
            _exp.RemoveAt(n);
            return x;
        }
    }
}
