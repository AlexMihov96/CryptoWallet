using CryptoWallet.Interfaces;
using NBitcoin;

namespace CryptoWallet.Core
{
    public class CommandFactory : ICommandFactory
    {
        private readonly Network currentNetwork;
        private readonly IWriter writer;
        private readonly IReader reader;

        public CommandFactory(IWriter writer, IReader reader, Network currentNetwork)
        {
            this.writer = writer;
            this.reader = reader;
            this.currentNetwork = currentNetwork;
        }

        public void PerformCommand(int input)
        {
            switch (input)
            {
                case (int)WalletOperations.Operations.Create:
                    this.CreateWallet();
                    break;
                case (int)WalletOperations.Operations.Recover:
                    this.RecoverWallet();
                    break;
                case (int)WalletOperations.Operations.Balance:
                    this.GetBalance();
                    break;
                case (int)WalletOperations.Operations.History:
                    this.ShowHistory();
                    break;
                case (int)WalletOperations.Operations.Receive:
                    this.Receive();
                    break;
                case (int)WalletOperations.Operations.Send:
                    this.Send();
                    break;
                case (int)WalletOperations.Operations.Exit:
                    this.writer.WriteLine("See ya :))");
                    break;
            }
        }

        public void WriteLineCommand(string message)
        {
            this.writer.WriteLine(message);
        }

        public void WriteCommand(string message)
        {
            this.writer.Write(message);
        }

        public string ReadLineCommand()
        {
            return this.reader.ReadLine();
        }

        private void CreateWallet()
        {
            string walletFilePath = @"Wallets\";
            string password = string.Empty;
            string passwordConfirmation = string.Empty;

            do
            {
                this.writer.Write("Enter password: ");
                password = this.reader.ReadLine();
                this.writer.Write("Confirm password: ");
                passwordConfirmation = this.reader.ReadLine();

                if (password != passwordConfirmation)
                {
                    this.writer.WriteLine("Password does not match!");
                    this.writer.WriteLine("Try again.");
                }

            } while (password != passwordConfirmation);
        }

        private void GetBalance()
        {
            throw new System.NotImplementedException();
        }

        private void Send()
        {
            throw new System.NotImplementedException();
        }

        private void Receive()
        {
            throw new System.NotImplementedException();
        }

        private void ShowHistory()
        {
            throw new System.NotImplementedException();
        }

        private void RecoverWallet()
        {
            throw new System.NotImplementedException();
        }
    }
}
