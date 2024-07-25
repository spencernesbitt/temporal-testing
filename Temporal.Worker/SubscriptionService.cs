namespace Temporal.Worker
{
    internal class SubscriptionService
    {
        public async Task<string> AddSubscriptionsAsync(string emailAddress, string subscriptionTypes)
        {
            var subs = subscriptionTypes.Split(';');
            foreach (var subscriptionType in subs)
            {
                Database.AddSubscription(emailAddress, subscriptionType);
            }
            return await Task.FromResult("Success");
        }
    }
}
