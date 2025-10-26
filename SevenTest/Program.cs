using System.Diagnostics;

namespace SevenTest;

class Program
{
    static void Main(string[] args)
    {
        StartTestDict();
        StartTestLargeDict();
    }
    
    public static void StartTestDict()
    {
        var len = 100_000_000;
  
        var sw = new Stopwatch();
        var dict = new Dictionary<long, long>(210);
        sw.Start();
        for (long i = 0; i < len; i++)
        {
            dict[i] = i;
            if (i % 1000000 == 0 && i != 0)
            {
                Console.WriteLine($"{i};{sw.Elapsed}");
                sw.Restart();
            }
        } 
    }

    public static void StartTestLargeDict()
    {
        var len = 100_000_000;
  
        var sw = new Stopwatch();
        var dict = new LargeDictionary<long, long>();
        sw.Start();
        for (long i = 0; i < len; i++)
        {
            dict[i] = i;
            if (i % 1000000 == 0 && i != 0)
            {
                Console.WriteLine($"{i};{sw.Elapsed}");
                sw.Restart();
            }
        } 
    }
}