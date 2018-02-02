using System;
using CryptoWallet.Interfaces;

namespace CryptoWallet
{
    public class Reader : IReader
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
