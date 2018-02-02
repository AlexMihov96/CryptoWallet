namespace CryptoWallet.Interfaces
{
    public interface IWriter
    {
        void Write(string message, params object[] parameters);

        void WriteLine(string message, params object[] parameters);
    }
}
