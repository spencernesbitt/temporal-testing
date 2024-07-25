using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Temporal.Worker
{
    internal class BankingActivities
    {
        [Activity]
        public static async Task<string> WithdrawAsync(string account, int amount, string reference)
        {
            // Parameter validation left to the underlying service
            var bankService = new BankingService("bank1.example.com");
            Console.WriteLine($"Withdrawing ${amount} from account {account}.");
            try
            {
                return await bankService.WithdrawAsync(account, amount, reference).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApplicationFailureException("Withdrawal failed", ex);
            }
        }

        [Activity]
        public static async Task<string> DepositAsync(string account, int amount, string reference)
        {
            var bankService = new BankingService("bank2.example.com");
            Console.WriteLine($"Depositing ${amount} into account {account}.");

            // Uncomment below and comment out the try-catch block below to simulate unknown failure
            /*
            return await bankService.DepositThatFailsAsync(details.TargetAccount, details.Amount, details.ReferenceId);
            */
            try
            {
                return await bankService.DepositAsync(account, amount, reference);
            }
            catch (Exception ex)
            {
                throw new ApplicationFailureException("Deposit failed", ex);
            }
        }

        [Activity]
        public static async Task<string> RefundAsync(string account, int amount, string reference)
        {
            var bankService = new BankingService("bank1.example.com");
            Console.WriteLine($"Refunding ${amount} to account {account}.");
            try
            {
                return await bankService.RefundAsync(account, amount, reference);
            }
            catch (Exception ex)
            {
                throw new ApplicationFailureException("Refund failed", ex);
            }
        }
    }
}
