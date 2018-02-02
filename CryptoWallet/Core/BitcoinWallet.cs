using CryptoWallet.Interfaces;

namespace CryptoWallet.Core
{
    public class BitcoinWallet : IWallet
    {
        private readonly ICommandFactory cmdFactory;

        public BitcoinWallet(ICommandFactory cmdFactory)
        {
            this.cmdFactory = cmdFactory;
        }

        public void Start()
        {
            int input = 0;

            while (!input.Equals((int)WalletOperations.Operations.Exit))
            {
                string menu = WalletOperations.PrintMenu();
                this.cmdFactory.WriteLineCommand(menu);

                input = int.Parse(this.cmdFactory.ReadLineCommand());

                this.cmdFactory.PerformCommand(input);
            }
        }
    }
}