using System.Numerics;

namespace Steganography
{
    public class LCG
    {
        private readonly BigInteger m;
        private BigInteger a;
        private BigInteger c;
        private readonly BigInteger x0;
        private int index = 0;
        private readonly int seed = 0;

        public List<BigInteger> X { get; private set; } = new List<BigInteger>();

        public LCG(BigInteger m, BigInteger x0)
        {
            this.m = m;
            this.x0 = x0 % m;
            Initialize();
        }

        public LCG(BigInteger m, BigInteger x0, int seed)
        {
            this.m = m;
            this.x0 = x0 % m;
            this.seed = seed;
            Initialize();
        }

        private void Initialize()
        {
            X.Clear();
            var p = Primes();
            var f = Factors(p);
            c = CalcC(p, f);
            a = f.Aggregate((x, y) => x * y);
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
            for (BigInteger i = 1; i < m; i++)
            {
                X.Add(((a * X[(int)i - 1]) + c) % m);
            }
        }

        public void SetIndex(int i) => index = i;

        public BigInteger Get(int i) => X[i];

        public BigInteger Next()
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
            for (BigInteger i = 0; i < m; i++)
            {
                list.Add((double)X[(int)i]);
            }
            return list;
        }

        private BigInteger CalcC(List<BigInteger> prime, List<BigInteger> factor)
        {

            var l = new List<BigInteger>();

            l.AddRange(prime);
            for (int i = 0; i < factor.Count; i++)
            {
                l.Remove(factor[i]);
            }

            if (l.Count > 0)
            {
                return l[new Random(seed).Next(0, l.Count)];
            }

            return 1;
        }

        private List<BigInteger> Factors(List<BigInteger> prime)
        {
            var f = new List<BigInteger>();
            foreach (var p in prime)
            {
                if (m % p == 0)
                {
                    f.Add(p);
                }
            }
            return f;
        }


        private List<BigInteger> Primes()
        {
            var p = Prime.GetPrimes(m);
            p.Add(1); //Add 1, needed if m == prime

            return p;
        }
    }
}
