using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Temporal.Worker
{
    internal class TenantActivities
    {
        [Activity]
        public static async Task<string> SendConfirmationEmailAsync(string emailAddress)
        {
            // Parameter validation left to the underlying service
            var emailService = new EmailService();
            Console.WriteLine($"Sending Confirmation Email to ${emailAddress}.");
            try
            {
                return await emailService.SendEmailConfirmationAsync(emailAddress).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApplicationFailureException("SendConfirmationEmail failed", ex);
            }
        }

        [Activity]
        public static async Task<string> AddSubscriptionsAsync(string emailAddress, string subscriptionTypes)
        {
            // Parameter validation left to the underlying service
            var subscriptionService = new SubscriptionService();
            Console.WriteLine($"Adding {subscriptionTypes} subscription to Email ${emailAddress}.");
            try
            {
                return await subscriptionService.AddSubscriptionsAsync(emailAddress, subscriptionTypes).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApplicationFailureException("AddSubscriptionAsync failed", ex);
            }
        }
    }
}
