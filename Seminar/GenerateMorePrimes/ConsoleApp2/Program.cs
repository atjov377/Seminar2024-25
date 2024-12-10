using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "C:/Users/vojte/source/repos/Primes.txt";// Path has to be adjusted based on pc
            List<ulong> Primes = new List<ulong>();
            using (StreamReader sr = new StreamReader(path)) 
            {
                while (sr.Peek() != -1)
                {
                    Primes.Add(ulong.Parse(sr.ReadLine()));
                }
            }
            Console.WriteLine("How many primes to generate");
            int n = int.Parse(Console.ReadLine());
            int PrimesCount = Primes.Count;
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                if (Primes.Count <= n)
                {
                    Primes = GeneratePrimes(n, Primes);
                    for (int i = PrimesCount; i < Primes.Count; i++)
                    {
                        sw.WriteLine(Primes[i].ToString());
                        Console.WriteLine(Primes[i]);
                    }
                }
            }
            Console.ReadLine();
        }
        public static List<ulong> GeneratePrimes(int n, List<ulong> Primes)
        {
            ulong NextPrimeCandidate;
            if (!Primes.Any())
            {
                Primes.Add(2);
                NextPrimeCandidate = 3;
            }
            else
            {
                NextPrimeCandidate = Primes[Primes.Count-1]+2;
            }
            while (Primes.Count <= n)
            {
                ulong Sqrt = (ulong)Math.Sqrt(NextPrimeCandidate);
                bool IsPrime = true;
                for (int i = 0; Primes[i] <= Sqrt; i++)
                {
                    if (NextPrimeCandidate % Primes[i] == 0)
                    {
                        IsPrime = false;
                        break;
                    }
                }
                if (IsPrime)
                {
                    Primes.Add(NextPrimeCandidate);
                }
                NextPrimeCandidate += 2;
            }
            return Primes;
        }
    }
}
