using CryptoWallet.Core;
using CryptoWallet.Interfaces;
using NBitcoin;

namespace CryptoWallet
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            IWriter writer = new Writer();
            IReader reader = new Reader();
            Network currentNetwork = Network.TestNet;

            ICommandFactory cmdFactory = new CommandFactory(writer, reader, currentNetwork);

            IWallet wallet = new BitcoinWallet(cmdFactory);

            wallet.Start();
        }
    }
}
