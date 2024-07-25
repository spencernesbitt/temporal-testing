namespace Temporal.Worker
{
    public record TenantDetails(
        string EmailAddress,
        string Subscriptions, // semi-colon delimited string
        string ReferenceId)
    { };

}
