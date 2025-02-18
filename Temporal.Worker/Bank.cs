﻿namespace Temporal.Worker
{
    internal class Bank(List<Account> accounts)
    {
        private readonly List<Account> accounts = accounts ?? [];

        public Account FindAccount(string accountNumber)
        {
            var account = accounts.FirstOrDefault(acc => acc.AccountNumber == accountNumber) ?? throw new InvalidAccountException($"The account number {accountNumber} is invalid.");
            return account;
        }
    }
}
