using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions
{
    public class HttpFunctions
    {
        [FunctionName("ApproveImage")]
        public async Task<IActionResult> ApproveImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "approve")] HttpRequest request,
            [DurableClient] IDurableClient client,
            ILogger logger)
        {
            using var streamReader = new StreamReader(request.Body);
            var jsonObject = JObject.Parse(await streamReader.ReadToEndAsync());
            var workflowName = jsonObject["workflowName"].Value<string>();

            var instance = await client.GetStatusAsync(workflowName);
            if (instance == null)
            {
                return new NotFoundObjectResult(null);
            }

            await client.RaiseEventAsync(instance.InstanceId, "ImageApproved", eventData: true);
            return new AcceptedResult();
        }
    }
}
