namespace Temporal.Worker
{
    internal class EmailService
    {
        public async Task<string> SendEmailConfirmationAsync(string emailAddress)
        {
            Database.AddSentEmail("EmailConfirmation", emailAddress);
            return await Task.FromResult("Success");
        }
    }
}
