using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions
{
    public class WorkflowCleanupFunctions
    {
        [FunctionName("PurgeWorkflowHistory")]
        public async Task PurgeWorkflowHistory(
            [ActivityTrigger]IDurableActivityContext context,
            [DurableClient]IDurableOrchestrationClient client)
        {
            await client.PurgeInstanceHistoryAsync(context.InstanceId);
        }
    }
}
