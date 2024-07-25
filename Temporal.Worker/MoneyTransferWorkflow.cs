
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace Temporal.Worker
{
    [Workflow]
    public class MoneyTransferWorkflow
    {
        [WorkflowRun]
        public async Task<string> RunAsync(PaymentDetails details)
        {
            var retryPolicy = new RetryPolicy
            {
                // Time to wait before the first retry
                InitialInterval = TimeSpan.FromSeconds(1),
                // Maximum wait time between retries (relevant when we have exponential backoff
                MaximumInterval = TimeSpan.FromSeconds(100),
                // Multiply the previous wait time by this factor to get the new wait time
                BackoffCoefficient = 2,
                // The maximum number of retries
                MaximumAttempts = 500,
                // A set of exceptions that we consider fatal for the workflow.
                NonRetryableErrorTypes = new[] { "InvalidAccountException", "InsufficientFundsException" }
            };

            string withdrawResult = "";
            try
            {
                withdrawResult = await Workflow.ExecuteActivityAsync(
                () => BankingActivities.WithdrawAsync(details.SourceAccount, details.Amount, details.ReferenceId),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                );
            }
            catch (ApplicationFailureException ex) when (ex.ErrorType == "InsufficientFundsException")
            {
                throw new ApplicationFailureException("Withdrawal failed due to insufficient funds.", ex);
            }

            string depositResult = "";
            try
            {
                depositResult = await Workflow.ExecuteActivityAsync(
                    () => BankingActivities.DepositAsync(details.TargetAccount, details.Amount, details.ReferenceId),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                    );
            }
            catch (ApplicationFailureException depositEx)
            {
                // The deposit failed, so try and refund the earlier withdrawal
                try
                {
                    string refundResult = await Workflow.ExecuteActivityAsync(
                        () => BankingActivities.RefundAsync(details.SourceAccount, details.Amount, details.ReferenceId),
                        new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                        );

                    // refund succeeded, but need to throw for the deposit failure
                    throw new ApplicationFailureException($"Failed to deposit money into account {details.TargetAccount}. Money returned to {details.SourceAccount}. Cause: {depositEx.Message}", depositEx);
                }
                catch (ApplicationFailureException refundEx)
                {
                    // both the deposit and refund failed
                    throw new ApplicationFailureException($"Failed to deposit money into account {details.TargetAccount}. Money could not be returned to {details.SourceAccount}. Cause: {refundEx.Message}", refundEx);
                }
            }

            // If everything succeeds, return transfer complete
            return $"Transfer complete (transaction IDs: {withdrawResult}, {depositResult})";
        }
    }
}