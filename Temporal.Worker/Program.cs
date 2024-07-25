using Temporal.Worker;

using Temporalio.Client;

using Temporalio.Worker;

// Create a client to connect to localhost on "default" namespace
var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

// Cancellation token to shutdown worker on ctrl+c
using var tokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    tokenSource.Cancel();
    eventArgs.Cancel = true;
};

// Create an instance of the activities since we have instance activities.
// If we had all static activities, we could just reference those directly.
var activities = new TenantActivities();

Console.WriteLine("Creating a worker to process jobs from TENANT_ONBOARDING_TASK_QUEUE");
// Create a worker with the activity and workflow registered
using var worker = new TemporalWorker(
    client, // client
    new TemporalWorkerOptions(taskQueue: "TENANT_ONBOARDING_TASK_QUEUE")
        .AddAllActivities(activities) // Register activities
        .AddWorkflow<OnboardTenantWorkflow>() // Register workflow
);

// Run the worker until it's cancelled
Console.WriteLine("Running worker...");
try
{
    await worker.ExecuteAsync(tokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Worker cancelled");
}