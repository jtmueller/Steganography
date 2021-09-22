namespace Steganography;

using System.Numerics;
using System.Text;

public static class Prime
{
    private static List<BigInteger> primeList = new();
    private static BigInteger index = 0;
    private static Thread? thread;
    private static Thread? loadThread;
    private static readonly string file = "primes.bin";
    private static StringBuilder sbuilder = new();
    private static int intent = 0;
    private static readonly int maxIntent = 300;
    private static BigInteger max = 1000000;
    private static readonly CancellationTokenSource cts = new();

    public static void Initialize()
    {
        primeList = new List<BigInteger>();
        
        loadThread = new Thread(static () => InitializeUsingFiles(cts.Token));
        loadThread.Start();
    }

    private static void StartGenerator(CancellationToken ct)
    {
        thread = new Thread(() => Generate(ct));
        thread.Start();
    }

    private static void InitializeUsingFiles(CancellationToken ct)
    {
        if (!File.Exists(file))
        {
            primeList.Add(2);
            primeList.Add(3);
            index = 5;
            File.WriteAllText(file, "2\r\n3\r\n");
            StartGenerator(ct);
        }
        else
        {
            foreach (string s in File.ReadLines(file))
            {
                ct.ThrowIfCancellationRequested();
                if (BigInteger.TryParse(s.TrimEnd('\r', '\n'), out var b))
                {
                    primeList.Add(b);
                }
                else
                {
                    primeList.Clear();
                    primeList.Add(2);
                    primeList.Add(3);
                    index = 5;
                    StartGenerator(ct);
                    return;
                }
            }
            index = primeList[^1];
            StartGenerator(ct);
        }
    }

    private static void Generate(CancellationToken ct)
    {
        for (var i = index; i < max && !ct.IsCancellationRequested; i += 2)
        {
            bool e = true;
            if (i % 2 == 0 || i % 3 == 0)
            {
                e = false;
            }
            for (BigInteger n = 5; n * n <= i; n += 6)
            {
                if (i % n == 0 || i % (n + 2) == 0)
                {
                    e = false;
                }
            }
            if (e)
            {
                lock (primeList)
                {
                    primeList.Add(i);
                    sbuilder.AppendLine(i.ToString());
                    intent++;
                    if (intent > maxIntent)
                    {
                        try
                        {
                            File.AppendAllText(file, sbuilder.ToString());
                            intent = 0;
                            sbuilder.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            index += 2;
        }
    }

    public static List<BigInteger> GetPrimes(BigInteger m)
    {
        if (max < m)
        {
            IncreaseMax(m);
        }
        while (index < m)
        {
            OutputConsole.Write($"Current index:{index}, needed:{m}");
            Thread.Sleep(1000);
        }
        var list = new List<BigInteger>();
        lock (primeList)
        {
            list = primeList.Where(n => n <= m).ToList();
        }

        return list;
    }

    public static void IncreaseMax(BigInteger nmax)
    {
        max = nmax;
        if (thread != null)
        {
            if (!thread.IsAlive)
            {
                thread = new Thread(() => Generate(cts.Token));
                thread.Start();
            }
        }
    }

    public static void Finish()
    {
        cts.Cancel();

        if (sbuilder.Length > 0)
        {
            File.AppendAllText(file, sbuilder.ToString());
        }
    }
}
