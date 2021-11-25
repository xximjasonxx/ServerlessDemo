using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageProcessor.Functions
{
    public class ReceiveImageFunction
    {
        [FunctionName("ReceiveImageFunction")]
        public async Task Run(
            [BlobTrigger("raw/{name}", Connection = "AzureWebJobsStorage")]Stream incomingBlob,
            string name,
            [Blob("original", FileAccess.Write, Connection = "AzureWebJobsStorage")] CloudBlobContainer container,
            ILogger log)
        {
            // todo: check the incoming mime-type

            // generate a name for the image uploaded
            var blobName = $"{Guid.NewGuid()}-{name}";
            log.LogInformation($"New image name will be {blobName}");

            // save the image to the original container
            var blobReference = container.GetBlockBlobReference(blobName);
            await blobReference.UploadFromStreamAsync(incomingBlob);
        }
    }
}
