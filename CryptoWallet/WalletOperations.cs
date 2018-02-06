namespace CryptoWallet
{
    public class WalletOperations
    {
        public enum Operations
        {
            Create = 1,
            Recover = 2,
            Balance = 3,
            History = 4,
            Receive = 5,
            Send = 6,
            Exit = 7
        }

        //Getting Enumerations key to display it's value, and getting enum's key to be displayed as well
        //E.g. Operations.Create => Create = 1, so we pick 1 and getting key's string representations e.g. "Create"
        public static string PrintMenu()
        {
            string menu = string
                    .Format(@"Choose an operation:" + "\n" +
                         $"{(int)Operations.Create}: {Operations.Create}\n" +
                         $"{(int)Operations.Recover}: {Operations.Recover}\n" +
                         $"{(int)Operations.Balance}: {Operations.Balance}\n" +
                         $"{(int)Operations.History}: {Operations.History}\n" +
                         $"{(int)Operations.Receive}: { Operations.Receive}\n" +
                         $"{(int)Operations.Send}: { Operations.Send}\n" +
                         $"{(int)Operations.Exit}: {Operations.Exit}\n" +
                         "---------------------"
            );

            return menu;
        }
    }
}