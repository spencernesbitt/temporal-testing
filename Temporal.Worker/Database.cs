using Newtonsoft.Json;

namespace Temporal.Worker
{
    public static class Database
    {
        private static Dictionary<string, string> tenants = new Dictionary<string, string>();// Tenant Email Address and Status
        private static List<KeyValuePair<string, string>> sentEmails = new List<KeyValuePair<string, string>>(); // Email type and email address
        private static List<KeyValuePair<string, string>> subscriptions = new List<KeyValuePair<string, string>>(); // Email address and subscription type
        public static void AddTenant(string emailAddress)
        {
            tenants.Add(emailAddress, "AwaitingEmailConfirmation");
        }

        public static void SetTenantStatus(string emailAddress, string status)
        {
            tenants[emailAddress] = status;
        }

        public static string GetTenantStatus(string emailAddress)
        {
            return tenants[emailAddress];
        }

        public static string GetTenants()
        {
            return JsonConvert.SerializeObject(tenants);
        }

        public static void AddSentEmail(string type, string emailAddress)
        {
            sentEmails.Add(new KeyValuePair<string, string>(type, emailAddress));
        }

        public static string GetSentEmails()
        {
            return JsonConvert.SerializeObject(sentEmails);
        }

        public static bool EmailHasBeenSentToAddress(string emailType, string emailAddress)
        {
            return sentEmails.Contains(new KeyValuePair<string, string>(emailType, emailAddress));
        }

        public static void AddSubscription(string emailAddress, string subscriptionType)
        {
            subscriptions.Add(new KeyValuePair<string, string>(emailAddress, subscriptionType));
        }

        public static string GetSubscriptions()
        {
            return JsonConvert.SerializeObject(subscriptions);
        }
    }
}
