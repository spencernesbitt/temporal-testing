namespace Temporal.Worker
{
    internal class Account
    {
        public string AccountNumber { get; private set; }
        public int Balance { get; set; }

        public Account(string accountNumber, int balance)
        {
            AccountNumber = accountNumber;
            Balance = balance;
        }
    }
}
