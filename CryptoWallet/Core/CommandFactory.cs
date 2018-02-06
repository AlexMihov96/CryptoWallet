using System;
using System.Globalization;
using CryptoWallet.Interfaces;
using HBitcoin.KeyManagement;
using NBitcoin;

namespace CryptoWallet.Core
{
    public class CommandFactory : ICommandFactory
    {
        private readonly Network currentNetwork;
        private readonly IWriter writer;
        private readonly IReader reader;
        private readonly string walletFilePath = @"Wallets\";

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
                    this.CreateWalletCommand();
                    break;
                case (int)WalletOperations.Operations.Recover:
                    this.RecoverCommand();
                    break;
                case (int)WalletOperations.Operations.Balance:
                    this.GetBalance();
                    break;
                case (int)WalletOperations.Operations.History:
                    this.ShowHistory();
                    break;
                case (int)WalletOperations.Operations.Receive:
                    this.ReceiveCommand();
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

        private void CreateWalletCommand()
        {
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

            bool failure = true;

            while (failure)
            {
                try
                {
                    this.WriteCommand("Enter wallet name: ");
                    string walletName = this.ReadLineCommand();

                    Mnemonic mnemonic;
                    string fileName = walletFilePath + walletName + ".json";
                    Safe safe = Safe.Create(out mnemonic, password, fileName, this.currentNetwork);
                    Console.WriteLine();
                    this.WriteLineCommand("Wallet created successfully");
                    this.WriteLineCommand("Write down the following mnemonic words.");
                    this.WriteLineCommand("With the mnemonic words AND the password you can recover this wallet.");
                    this.WriteLineCommand("");
                    this.WriteLineCommand("---------------");
                    this.WriteLineCommand(mnemonic.ToString());
                    this.WriteLineCommand("---------------");
                    this.WriteLineCommand("Write down and keep them SECURED, please keep your private keys. Only through them you can access your coins!");
                    this.WriteLineCommand("");

                    const int firstTenAddressesInWallet = 10;
                    for (int i = 0; i < firstTenAddressesInWallet; i++)
                    {
                        this.WriteLineCommand($"Address: {safe.GetAddress(i)} -> Private key: " +
                                              $"{safe.FindPrivateKey(safe.GetAddress(i))}");
                    }

                    failure = false;
                }
                catch
                {
                    this.WriteLineCommand("");
                    this.WriteLineCommand("Wallet already exists");
                    this.WriteLineCommand("");
                }
            }
        }

        private void GetBalance()
        {
            throw new System.NotImplementedException();
        }

        private void Send()
        {
            throw new System.NotImplementedException();
        }

        private void ReceiveCommand()
        {
            this.WriteCommand("Enter wallet's name: ");
            string walletName = this.ReadLineCommand();

            this.WriteCommand("Enter password: ");
            string password = this.ReadLineCommand();

            this.Receive(password, walletName);
        }

        private void Receive(string password, string walletName)
        {
            try
            {
                Safe loadedSafe = Safe.Load(password, this.walletFilePath + walletName + ".json");

                const int firstTenAddressesInWallet = 10;
                for (int i = 0; i < firstTenAddressesInWallet; i++)
                {
                    this.WriteLineCommand($"{i + 1}: " + loadedSafe.GetAddress(i));
                }
            }
            catch (Exception)
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Wallet with such name does not exist!");
                this.WriteLineCommand("");
            }
        }

        private void ShowHistory()
        {
            throw new System.NotImplementedException();
        }

        private void RecoverCommand()
        {
            this.WriteLineCommand("Please not that the wallet cannot check if your password is correct or not. " +
                                  "If you provide a wrong password a wallet will be recovered with your " +
                                  "provided mnemonic and password pair: ");
            this.WriteLineCommand("");

            this.WriteCommand("Enter password: ");
            string password = this.ReadLineCommand();

            this.WriteCommand("Enter your 12 words phrase: ");
            string mnemonic = this.ReadLineCommand();

            this.WriteCommand("Enter date (yyyy-MM-dd): ");
            string date = this.ReadLineCommand();

            Mnemonic mnem = new Mnemonic(mnemonic);

            this.Recover(password, mnem, date);
        }

        private void Recover(string password, Mnemonic mnem, string date)
        {
            Random random = new Random();

            Safe safe = Safe.Recover(mnem, password,
                this.walletFilePath + "RecoveredWalletNum" + random.Next() + ".json",
                this.currentNetwork,
                DateTimeOffset.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture));

            this.WriteLineCommand("Wallet successfully recovered");
            this.WriteLineCommand("");
        }
    }
}
