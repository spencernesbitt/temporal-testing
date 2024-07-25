using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace Temporal.Worker
{
    [Workflow]
    public class OnboardTenantWorkflow
    {
        // We can have a continuously running workflow that will wait for a new tenant request but
        // this might eventually exhaust the history limits etc. So assuming a new instance per tenant and
        // we'll signal when email has been verified etc.

        // We'll wait on this to be true before adding the subscriptions
        private bool emailHasBeenVerified = false;


        [WorkflowRun]
        public async Task<string> RunAsync(TenantDetails details)
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
                NonRetryableErrorTypes = new[] { "InvalidEmailTypeException", "InvalidSubscriptionTypeException" }
            };

            // We may have been restarted via a Signal, so See if we have sent an email already
            // ! NO don't do this, temporal will already know if we successfully didi this step. we can always check in the 
            // underlying service.
            //var confirmationEmailSent = Database.EmailHasBeenSentToAddress("EmailConfirmation", details.EmailAddress);
            //if (!confirmationEmailSent)
            //{
            var workflowInfo = Workflow.Info.WorkflowId;
            var logger = Workflow.Logger;
            Console.WriteLine($"Current Workflow info:{JsonConvert.SerializeObject(workflowInfo)}");
            logger.BeginScope(workflowInfo);
            logger.LogInformation($"Sending Confirmation Email to {details.EmailAddress}");
            string sendEmailResult = "";
            try
            {
                sendEmailResult = await Workflow.ExecuteActivityAsync(
                () => TenantActivities.SendConfirmationEmailAsync(details.EmailAddress),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                );
            }
            catch (ApplicationFailureException ex) when (ex.ErrorType == "InsufficientFundsException")
            {
                throw new ApplicationFailureException("Withdrawal failed due to insufficient funds.", ex);
            }

            //}
            Console.WriteLine($"Waiting for Confirmation from {details.EmailAddress}");
            logger.LogInformation($"Waiting for Confirmation from {details.EmailAddress}");
            // Wait for email confirmation before continuing
            await Workflow.WaitConditionAsync(() => emailHasBeenVerified);

            logger.LogInformation($"Email {details.EmailAddress} confirmed, Adding subscriptions");

            string subscriptionResult = "";

            try
            {
                subscriptionResult = await Workflow.ExecuteActivityAsync(
                    () => TenantActivities.AddSubscriptionsAsync(details.EmailAddress, details.Subscriptions),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                    );
            }
            catch (ApplicationFailureException subsEx)
            {
                // We could add compensations here (or run them here) instead of the 'manual' approach below
                Console.WriteLine($"Adding subs {details.Subscriptions} for {details.EmailAddress} failed", subsEx);
                // The deposit failed, so try and refund the earlier withdrawal
                //try
                //{
                //    string refundResult = await Workflow.ExecuteActivityAsync(
                //        () => BankingActivities.RefundAsync(details.SourceAccount, details.Amount, details.ReferenceId),
                //        new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy }
                //        );

                //    // refund succeeded, but need to throw for the deposit failure
                //    throw new ApplicationFailureException($"Failed to deposit money into account {details.TargetAccount}. Money returned to {details.SourceAccount}. Cause: {depositEx.Message}", depositEx);
                //}
                //catch (ApplicationFailureException refundEx)
                //{
                //    // both the deposit and refund failed
                //    throw new ApplicationFailureException($"Failed to deposit money into account {details.TargetAccount}. Money could not be returned to {details.SourceAccount}. Cause: {refundEx.Message}", refundEx);
                //}
                throw;
            }

            // If everything succeeds, return transfer complete
            return $"Tenant onboarding complete (Activity Results: {sendEmailResult}, {subscriptionResult})";
        }

        [WorkflowSignal]
        public async Task EmailVerified(string emailAddress)
        {
            // Ignoring the email adress but may want to verify it in a propor situation 
            emailHasBeenVerified = true;
        }
    }
}
