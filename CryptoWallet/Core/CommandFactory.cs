using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CryptoWallet.Interfaces;
using HBitcoin.KeyManagement;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

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
                    this.GetBalanceCommand();
                    break;
                case (int)WalletOperations.Operations.History:
                    this.ShowHistoryCommand();
                    break;
                case (int)WalletOperations.Operations.Receive:
                    this.ReceiveCommand();
                    break;
                case (int)WalletOperations.Operations.Send:
                    this.SendCommand();
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
                    Safe wallet = Safe.Create(out mnemonic, password, fileName, this.currentNetwork);
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
                        this.WriteLineCommand($"Address: {wallet.GetAddress(i)} -> Private key: " +
                                              $"{wallet.FindPrivateKey(wallet.GetAddress(i))}");
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

        private void GetBalanceCommand()
        {
            this.WriteCommand("Enter wallet's name: ");
            string walletName = this.ReadLineCommand();

            this.WriteCommand("Enter password: ");
            string password = this.ReadLineCommand();

            this.WriteCommand("Enter wallet address: ");
            string walletAddress = this.ReadLineCommand();

            this.GetBalance(password, walletName, walletAddress);
        }

        private void GetBalance(string password, string walletName, string walletAddress)
        {
            try
            {
                Safe wallet = Safe.Load(password, this.walletFilePath + walletName + ".json");
            }
            catch
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Wrong wallet or password");
                this.WriteLineCommand("");
            }

            QBitNinjaClient client = new QBitNinjaClient(this.currentNetwork);

            decimal totalBalance = 0;
            var balance = client.GetBalance(BitcoinAddress.Create(walletAddress), true).Result;

            foreach (var entry in balance.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;
                    decimal currentAmount = amount.ToDecimal(MoneyUnit.BTC);
                    totalBalance += currentAmount;
                }
            }

            this.WriteLineCommand("---------------------");
            this.WriteLineCommand($"Balance of wallet: {totalBalance}");
            this.WriteLineCommand("---------------------");
            this.WriteLineCommand("");
        }

        private void SendCommand()
        {
            this.WriteCommand("Enter wallet's name: ");
            string walletName = this.ReadLineCommand();

            this.WriteCommand("Enter wallet's password: ");
            string password = this.ReadLineCommand();

            this.WriteCommand("Enter wallet address: ");
            string walletAddress = this.ReadLineCommand();

            this.WriteCommand("Select outpoint (transaction ID): ");
            string outpoint = this.ReadLineCommand();

            this.Send(password, walletName, walletAddress, outpoint);
        }

        private void Send(string password, string walletName, string walletAddress, string outpoint)
        {
            BitcoinExtKey privateKey = null;

            try
            {
                Safe wallet = Safe.Load(password, this.walletFilePath + walletName + ".json");

                const int firstTenAddressesInWallet = 10;
                for (int i = 0; i < firstTenAddressesInWallet; i++)
                {
                    if (wallet.GetAddress(i).ToString() == walletAddress)
                    {
                        this.WriteLineCommand("");
                        this.WriteCommand("Enter private key: ");
                        privateKey = new BitcoinExtKey(this.ReadLineCommand());

                        if (!privateKey.Equals(wallet.FindPrivateKey(wallet.GetAddress(i))))
                        {
                            this.WriteLineCommand("");
                            this.WriteLineCommand("Wrong private key!");
                            this.WriteLineCommand("");
                        }

                        break;
                    }
                }
            }
            catch
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Wrong wallet or password");
                this.WriteLineCommand("");
            }

            QBitNinjaClient client = new QBitNinjaClient(this.currentNetwork);
            var balance = client.GetBalance(BitcoinAddress.Create(walletAddress), false).Result;
            OutPoint outpointToSpend = null;

            //outpoint = transaction ID
            foreach (var entry in balance.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    if (coin.Outpoint.ToString().Substring(0, coin.Outpoint.ToString().Length - 2) == outpoint)
                    {
                        outpointToSpend = coin.Outpoint;
                        break;
                    }
                }
            }

            this.MakeTransaction(outpointToSpend, privateKey, client);
        }

        private void MakeTransaction(OutPoint outpointToSpend, BitcoinExtKey privateKey, QBitNinjaClient client)
        {
            var transaction = new Transaction();

            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outpointToSpend
            });

            this.WriteCommand("Enter address to send to: ");
            string recieverAddress = this.ReadLineCommand();

            var hallOfTheMakersAddress = BitcoinAddress.Create(recieverAddress);

            this.WriteCommand("Enter amount to send: ");
            decimal amountToSend = decimal.Parse(this.ReadLineCommand());

            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = new Money(amountToSend, MoneyUnit.BTC),
                ScriptPubKey = hallOfTheMakersAddress.ScriptPubKey
            };

            this.WriteCommand("Enter amount the get back: ");
            decimal amountToGetBack = decimal.Parse(this.ReadLineCommand());

            TxOut changeBackTxOut = new TxOut()
            {
                Value = new Money(amountToGetBack, MoneyUnit.BTC),
                ScriptPubKey = privateKey.ScriptPubKey
            };

            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);

            this.WriteCommand("Enter message: ");
            string message = this.ReadLineCommand();
            var bytes = Encoding.UTF8.GetBytes(message);

            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });

            transaction.Inputs[0].ScriptSig = privateKey.ScriptPubKey;
            transaction.Sign(privateKey, false);

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (broadcastResponse.Success)
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Transaction Send!");
                this.WriteLineCommand("");
            }
            else
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Something went wrong! :'(");
                this.WriteLineCommand("");
            }
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
                Safe wallet = Safe.Load(password, this.walletFilePath + walletName + ".json");

                const int firstTenAddressesInWallet = 10;
                for (int i = 0; i < firstTenAddressesInWallet; i++)
                {
                    this.WriteLineCommand($"{i + 1}: " + wallet.GetAddress(i));
                }
            }
            catch (Exception)
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Wallet with such name does not exist!");
                this.WriteLineCommand("");
            }
        }

        private void ShowHistoryCommand()
        {
            this.WriteCommand("Enter wallet's name: ");
            string walletName = this.ReadLineCommand();

            this.WriteCommand("Enter password: ");
            string password = this.ReadLineCommand();

            this.WriteCommand("Enter wallet address: ");
            string walletAddress = this.ReadLineCommand();

            this.ShowHistory(password, walletName, walletAddress);
        }

        private void ShowHistory(string password, string walletName, string walletAddress)
        {
            try
            {
                Safe wallet = Safe.Load(password, this.walletFilePath + walletName + ".json");
            }
            catch
            {
                this.WriteLineCommand("");
                this.WriteLineCommand("Wrong wallet or password");
                this.WriteLineCommand("");
            }

            QBitNinjaClient client = new QBitNinjaClient(this.currentNetwork);

            var coinsReceived = client.GetBalance(BitcoinAddress.Create(walletAddress), true).Result;
            string header = "------COINS RECEIVED------";
            this.WriteLineCommand(header);

            foreach (var entry in coinsReceived.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;
                    this.WriteLineCommand
                        ($"Transaction ID: {coin.Outpoint}; Received Coins: {amount.ToDecimal(MoneyUnit.BTC)}");
                }
            }

            this.WriteLineCommand(new string('-', header.Length));

            var coinsSpent = client.GetBalance(BitcoinAddress.Create(walletAddress), false).Result;

            string footer = "------COINS SPENT------";
            this.WriteLineCommand(footer);

            foreach (var entry in coinsSpent.Operations)
            {
                foreach (var coin in entry.SpentCoins)
                {
                    Money amount = (Money)coin.Amount;
                    this.WriteLineCommand
                        ($"Transaction ID: {coin.Outpoint}; Spent Coins: {amount.ToDecimal(MoneyUnit.BTC)}");
                }
            }

            this.WriteLineCommand(new string('-', footer.Length));
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

            Safe wallet = Safe.Recover(mnem, password,
                this.walletFilePath + "RecoveredWalletNum" + random.Next() + ".json",
                this.currentNetwork,
                DateTimeOffset.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture));

            this.WriteLineCommand("Wallet successfully recovered");
            this.WriteLineCommand("");
        }
    }
}
