using System;
using System.IO;
using System.Threading.Tasks;
using ImageProcessorDurable.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace ImageProcessorDurable.Functions
{
    public class ProcessImageWorkflow
    {
        [FunctionName("ProcessImageWorkflow")]
        public async Task RunWorkflow(
            [BlobTrigger("raw/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
            [DurableClient]IDurableClient client,
            string name,
            ILogger logger)
        {
            // generate a unique name for our workflow
            var workflowName = $"process-{name}";
            var workflowInstance = await client.GetStatusAsync(workflowName);
            if (workflowInstance != null)
            {
                logger.LogWarning($"Attempted to start duplicative workflow {workflowName}");
                await client.PurgeInstanceHistoryAsync(workflowName);
            }

            logger.LogInformation($"Starting Workflow with identifier {workflowName}");
            var workflowInput = new WorkflowInput
            {
                OriginalName = name,
                BlobData = myBlob.AsBase64String()
            };
            
            await client.StartNewAsync("OrchestarteWorkflow", workflowName, workflowInput);
        }

        [FunctionName("OrchestarteWorkflow")]
        public async Task OrchestrateProcessWorkflow(
            [OrchestrationTrigger]IDurableOrchestrationContext context,
            ILogger logger)
        {
            // get the byte data
            var input = context.GetInput<WorkflowInput>();

            // send to cognitive services to check if image is prohibited
            var needsValidation = await context.CallActivityAsync<bool>("GetImageIsProhibited", input.BlobData);
            if (needsValidation)
            {
                logger.LogInformation($"Waiting for approval for workflow {context.InstanceId}");
                var uploadApprovedEvent = context.WaitForExternalEvent<bool>("ImageApproved");
                await Task.WhenAny(uploadApprovedEvent);
            }

            // save the image in its original format and a resized variant
            var processInput = new ProcessImageInput
            {
                Name = $"{Guid.NewGuid()}-{input.OriginalName}",
                RawBlobData = input.BlobData
            };

            var saveImageTask = context.CallActivityAsync("SaveImage", processInput);
            var resizeImageTask = context.CallActivityAsync("ResizeImage", processInput);
            await Task.WhenAll(new[] {
                saveImageTask,
                resizeImageTask
            });

            await context.CallActivityAsync("PurgeWorkflowHistory", null);
        }
    }
}
