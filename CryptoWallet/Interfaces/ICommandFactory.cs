namespace CryptoWallet.Interfaces
{
    public interface ICommandFactory
    {
        void PerformCommand(int input);

        void WriteLineCommand(string message);

        void WriteCommand(string message);

        string ReadLineCommand();
    }
}
