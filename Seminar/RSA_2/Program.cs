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
        private string PrimePath = "C:/Users/vojte/source/repos/Primes.txt"; // Path has to be adjusted based on pc // Path of text file of Prime numbers
        private string EncryptPath = "C:/Users/vojte/source/repos/Message to encrypt.txt"; // Path has to be adjusted based on pc // Path of text file to be encrypted
        private string DecryptPath = "C:/Users/vojte/source/repos/Encrypted message.txt"; // Path has to be adjusted based on pc // Path of text file to be Decrypted / path of encrypted file
        private string EndMessagePath = "C:/Users/vojte/source/repos/Decrypted message.txt"; // Path has to be adjusted based on pc // Path of text file with end message
        private string PublicKeysPath = "C:/Users/vojte/source/repos/Public keys.txt"; // Path has to be adjusted based on pc // Path of text file with public keys

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
                while (sr.Peek() != -1)
                {
                    Primes.Add(BigInteger.Parse(sr.ReadLine()));
                }
            }

            if (MinPrime >= Primes.Count - 1) { Console.WriteLine("Your minimal Prime number position is too large, please select a lower minimal prime number position or generate more prime numbers"); Console.ReadLine(); throw new Exception(); }

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
        public static BigInteger Pow(BigInteger value, BigInteger exponent)
        {
            BigInteger originalValue = value;
            while (exponent-- > 1)
            {
                value = BigInteger.Multiply(value, originalValue);
                Console.WriteLine(exponent);
            }
                
            return value;
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
}