using System;
using CryptoWallet.Interfaces;

namespace CryptoWallet
{
    public class Writer : IWriter
    {
        public void Write(string message, params object[] parameters)
        {
            Console.Write(message, parameters);
        }

        public void WriteLine(string message, params object[] parameters)
        {
            Console.WriteLine(message, parameters);
        }
    }
}
