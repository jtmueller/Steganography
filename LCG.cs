namespace Steganography
{
    public class LCG
    {
        private readonly uint m;
        private uint a;
        private uint c;
        private readonly uint x0;
        private int index = 0;
        private readonly int seed = 0;

        public List<uint> X { get; private set; } = new List<uint>();

        public LCG(uint m, uint x0)
        {
            this.m = m;
            this.x0 = x0 % m;
            Initialize();
        }

        public LCG(int m, int x0) : this((uint)m, (uint)x0) { }

        public LCG(uint m, uint x0, int seed)
        {
            this.m = m;
            this.x0 = x0 % m;
            this.seed = seed;
            Initialize();
        }

        public LCG(int m, int x0, int seed) : this((uint)m, (uint)x0, seed) { }

        private void Initialize()
        {
            X.Clear();
            c = CalcC(Primes(), out var factors);
            a = factors.Aggregate((x, y) => x * y);
            if (m % 4 == 0)
            {
                if (a % 4 == 0)
                {
                    a++;
                }
                else
                {
                    a = (a * 4) + 1;
                }
            }
            else
            {
                a++;
            }

            index = 0;
            X.Add(x0);
            for (uint i = 1; i < m; i++)
            {
                X.Add(((a * X[(int)i - 1]) + c) % m);
            }
        }

        public void SetIndex(int i) => index = i;

        public uint Get(int i) => X[i];

        public uint Next()
        {
            var r = X[index];
            index++;
            if (index > X.Count)
            {
                index = 0;
            }
            return r;
        }

        public List<double> GetList()
        {
            var list = new List<double>();
            for (uint i = 0; i < m; i++)
            {
                list.Add(X[(int)i]);
            }
            return list;
        }

        private uint CalcC(IEnumerable<uint> primes, out List<uint> factors)
        {
            var random = new Random(seed);

            var fs = new List<uint>();

            var result = primes
                .Where(p =>
                {
                    if (m % p == 0)
                    {
                        fs.Add(p);
                        return false;
                    }
                    return true;
                })
                .RandomOrDefault(random, 1u);

            factors = fs;
            return result;
        }

        private IEnumerable<uint> Primes()
        {
            //Add 1, needed if m == prime
            return Sieve32.Primes(m).Append(1u);
        }
    }
}
