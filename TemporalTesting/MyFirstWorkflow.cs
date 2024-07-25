using Temporalio.Workflows;

namespace TemporalTesting
{
    [Workflow]
    public class MyFirstWorkflow
    {
        public async Task<string> RunAsync(string name)
        {
            return string.Empty;
        }
    }
}
