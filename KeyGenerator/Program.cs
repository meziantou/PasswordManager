using System;
using System.Security.Cryptography;

namespace KeyGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var length = 256;
            if (args.Length > 0 && int.TryParse(args[0], out var v))
            {
                length = v;
            }

            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length / 8];
            rng.GetBytes(bytes);
            Console.WriteLine(Convert.ToBase64String(bytes));
        }
    }
}
