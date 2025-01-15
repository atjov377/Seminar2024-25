using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RSA_1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RSA rsa = new RSA();
            rsa.StartRSA();
        }
    }
    class RSA
    {
        private string InstanceName; // Name of instance to be able to send messages between diferent instances
        private BigInteger Prime1 = 0;
        private BigInteger Prime2 = 0;
        private BigInteger PublicPrimesMultiplied;   // First part of public key
        private BigInteger PublicCypherExponent;     // Second part of public key
        private BigInteger DecryptExponent;          // Private key
        Random rng = new Random();
        List<BigInteger> Primes = new List<BigInteger>();
        private int MinPrime;
        private string PrimePath = "Primes.txt"; // Path has to be adjusted based on pc // Path of text file of Prime numbers
        private string EncryptPath = "Message to encrypt.txt"; // Path has to be adjusted based on pc // Path of text file to be encrypted
        private string DecryptPath = "Encrypted message.txt"; // Path has to be adjusted based on pc // Path of text file to be Decrypted / path of encrypted file
        private string EndMessagePath = "Decrypted message.txt"; // Path has to be adjusted based on pc // Path of text file with end message
        private string PublicKeysPath = "Public keys.txt"; // Path has to be adjusted based on pc // Path of text file with public keys

        public void StartRSA()
        {
            Console.WriteLine("Do you want to Clear Public keys?");
            string Clear = Console.ReadLine();
            if (Clear == "Yes" || Clear == "yes")
            {
                File.WriteAllText(PublicKeysPath, "");
            }
            Console.WriteLine("What is the name of this instance?");
            InstanceName = Console.ReadLine();
            Console.WriteLine("What is the minimal Prime number position?");
            MinPrime = int.Parse(Console.ReadLine()); //Minimal prime number position
            GenerateKeys();
            Console.WriteLine("Now you can start your other instance and than press enter to continue");
            Console.ReadLine();
            while (true)
            {
                Console.WriteLine("Do you want to Encrypt or Decrypt a message or do you want to End program?");
                string Input = Console.ReadLine();
                if (Input.ToLower() == "end program")
                {
                    break;
                }
                if (Input.ToLower() == "encrypt")
                {
                    Console.WriteLine("Who is the message to?"); // User types instance name that he wants to encrypt to
                    string address = Console.ReadLine();
                    using (StreamReader sr = new StreamReader(EncryptPath))
                    {
                        string message = sr.ReadToEnd();
                        Encrypt(message, address);
                        Console.WriteLine("Succesfully encrypted");
                    }
                }
                else if (Input.ToLower() == "decrypt")
                {
                    using (StreamReader sr = new StreamReader(DecryptPath))
                    {
                        string[] stringMessage = sr.ReadToEnd().Split();
                        List<BigInteger> message = new List<BigInteger>();
                        for (int i = 0; i < stringMessage.Length - 1; i++)
                        {
                            Console.WriteLine(stringMessage[i]);
                            message.Add(BigInteger.Parse(stringMessage[i]));
                        }
                        Decrypt(message);
                        Console.WriteLine("Succesfully decrypted");
                    }
                }
            }
        }
        public void GenerateKeys()
        {
            using (StreamReader sr = new StreamReader(PrimePath))
            {
                Console.WriteLine("Please wait for keys to be generated");
                while (sr.Peek() != -1)
                {
                    Primes.Add(BigInteger.Parse(sr.ReadLine()));
                }
            }

            if (MinPrime >= Primes.Count - 1) 
            { 
                Console.WriteLine("Your minimal Prime number position is too large, please select a lower minimal prime number position or generate more prime numbers");
                Console.WriteLine("Do you want to generate more primes?");
                if (Console.ReadLine().ToLower() == "yes")
                {
                    PrimesGeneration primesGeneration = new PrimesGeneration();
                    primesGeneration.Generate(PrimePath);
                    Console.WriteLine("Your primes have been generated, please restart the program to continue");
                    Console.ReadLine();
                }
                throw new Exception(); 
            }

            Prime1 = Primes[rng.Next(MinPrime, Primes.Count - 1)];
            Prime2 = Primes[rng.Next(MinPrime, Primes.Count - 1)];
            while (Prime2 == Prime1) Prime2 = Primes[rng.Next(MinPrime, Primes.Count - 1)];

            PublicPrimesMultiplied = Prime1 * Prime2;
            BigInteger EulerFunction = LeastCommonMultiple(Prime1 - 1, Prime2 - 1);
            PublicCypherExponent = ulong.MaxValue;
            while (PublicCypherExponent >= EulerFunction)
            {
                PublicCypherExponent = Primes[rng.Next(0, Primes.Count - 1)];
            }
            DecryptExponent = 2;
            while (true)
            {
                if ((DecryptExponent * PublicCypherExponent) % EulerFunction == 1) break; DecryptExponent++;
            }
            bool HadPublicKey = false;
            StringBuilder PublicKeys = new StringBuilder();
            using (StreamReader sr = new StreamReader(PublicKeysPath))
            {
                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();
                    string[] lineArray = line.Split();
                    if (lineArray[0] == InstanceName) HadPublicKey = true;
                    else PublicKeys.Append(line);
                }
            }
            if (HadPublicKey)
            {
                using (StreamWriter sw = new StreamWriter(PublicKeysPath))
                {
                    sw.Write(PublicKeys.ToString());
                    sw.WriteLine(InstanceName + " " + PublicPrimesMultiplied + " " + PublicCypherExponent);
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(PublicKeysPath, true))
                {
                    sw.WriteLine(InstanceName + " " + PublicPrimesMultiplied + " " + PublicCypherExponent);
                }
            }
        }
        public void Encrypt(string message, string address)
        {
            BigInteger PrimesMultiplied = 0;
            BigInteger CypherExponent = 0;
            using (StreamReader sr = new StreamReader(PublicKeysPath))
            {
                while (sr.Peek() != -1)
                {
                    string[] lineArray = sr.ReadLine().Split();
                    if (lineArray[0] == address)
                    {
                        PrimesMultiplied = BigInteger.Parse(lineArray[1]);
                        CypherExponent = BigInteger.Parse(lineArray[2]);
                        break;
                    }
                }
                if (PrimesMultiplied == 0) { Console.WriteLine("This address does not exist"); return; }
            }
            Console.WriteLine(PrimesMultiplied);
            Console.WriteLine(CypherExponent);
            using (StreamWriter sw = new StreamWriter(DecryptPath))
            {
                for (int i = 0; i < message.Length; i++) { sw.Write(BigInteger.ModPow(message[i], CypherExponent, PrimesMultiplied)); sw.Write(" "); }
            }
        }
        public void Decrypt(List<BigInteger> message)
        {
            Console.WriteLine(string.Join(" ", message));
            Console.WriteLine(DecryptExponent);
            Console.WriteLine(PublicPrimesMultiplied);

            using StreamWriter sw = new StreamWriter(EndMessagePath);
            for (int i = 0; i < message.Count; i++)
            {
                sw.Write((char)BigInteger.ModPow(message[i], DecryptExponent, PublicPrimesMultiplied));
            }

        }
        public static BigInteger LeastCommonMultiple(BigInteger a, BigInteger b)
        {
            BigInteger Num1;
            BigInteger Num2;
            if (a < b)
            {
                Num1 = b;
                Num2 = a;
            }
            else
            {
                Num1 = a;
                Num2 = b;
            }
            for (int i = 1; i < Num2; i++)
            {
                BigInteger x = Num1 * i;
                if (x % Num2 == 0)
                {
                    return x;
                }
            }
            return a * b;
        }
    }
    class PrimesGeneration
    {
        public void Generate(string path)
        {
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
                    }
                }
            }
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
                NextPrimeCandidate = Primes[Primes.Count - 1] + 2;
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