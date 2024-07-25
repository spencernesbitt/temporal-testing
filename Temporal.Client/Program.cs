// See https://aka.ms/new-console-template for more information
using Temporal.Worker;

using Temporalio.Client;

// Decide which task to run based on the incoming arguments
switch (args.ElementAtOrDefault(0))
{
    case "signal":
        await SendEmailVerifiedNotification();
        break;
    case "workflow":
        await QueueWorkflow();
        break;
    default:
        throw new ArgumentException("Must pass 'worker' or 'workflow' as the single argument");
}


// Task to queue a Workflow job
async Task QueueWorkflow()
{
    Console.WriteLine("Hello, World!");

    // Connect to the Temporal server
    var client = await TemporalClient.ConnectAsync(new("localhost:7233") { Namespace = "default" });

    // Define payment details
    /*
    var details = new PaymentDetails(
        SourceAccount: "85-150",
        TargetAccount: "43-812",
        Amount: 400,
        ReferenceId: "12345"
    );
    */
    // Define tenant details
    var tenantDetails = new TenantDetails
    (
        EmailAddress: "tenant.admin@tenant1.com",
        Subscriptions: "SUB1;SUB2;SUB3;SUB4",
        ReferenceId: Guid.NewGuid().ToString()
    );

    Console.WriteLine($"Queing a WF request for {tenantDetails.EmailAddress} on TENANT_ONBOARDING_TASK_QUEUE");
    //Console.WriteLine($"Starting transfer from account {details.SourceAccount} to account {details.TargetAccount} for ${details.Amount}");

    var workflowId = $"onboard-tenant-{tenantDetails.EmailAddress}";

    try
    {
        // Start the workflow
        var handle = await client.StartWorkflowAsync(
            (OnboardTenantWorkflow wf) => wf.RunAsync(tenantDetails),
            new(id: workflowId, taskQueue: "TENANT_ONBOARDING_TASK_QUEUE"));

        Console.WriteLine($"Started Workflow {workflowId}");

        // Await the result of the workflow
        var result = await handle.GetResultAsync();
        Console.WriteLine($"Workflow result: {result}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Workflow execution failed: {ex.Message}");
    }
}

// Task to send the workflow a signal that the email address has been verified
async Task SendEmailVerifiedNotification()
{
    // Connect to the Temporal server
    var client = await TemporalClient.ConnectAsync(new("localhost:7233") { Namespace = "default" });
    var workflowHandle = client.GetWorkflowHandle<OnboardTenantWorkflow>($"onboard-tenant-tenant.admin@tenant1.com");

    Console.WriteLine("Sending Signal: Email Verified for tenant.admin@tenant1.com");

    await workflowHandle.SignalAsync(wf => wf.EmailVerified("tenant.admin@tenant1.com"));

    Console.WriteLine("Signal Sent");
    /*
    var handle = await client.StartWorkflowAsync(
       (LoyaltyProgram wf) => wf.RunAsync("user-id-123"),
       new(id: "signals-queries-workflow-id", taskQueue: "signals-queries-sample"));
    */
}