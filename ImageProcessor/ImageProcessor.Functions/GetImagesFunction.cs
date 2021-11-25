using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ImageProcessor.Functions
{
    public class GetImagesFunction
    {
        [FunctionName("GetImages")]
        public async Task<IActionResult> GetImages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images")] HttpRequest req,
            [Blob("resized", FileAccess.Read, Connection = "AzureWebJobsStorage")] CloudBlobContainer container,
            ILogger log)
        {
            var blobResults = await container.ListBlobsSegmentedAsync(null);
            return new OkObjectResult(blobResults.Results
                .Select(x => new
                {
                    SourcePath = x.StorageUri.PrimaryUri,
                    Name = Path.GetFileName(x.StorageUri.PrimaryUri.LocalPath)
                }).ToList());
        }

        [FunctionName("GetImage")]
        public async Task<IActionResult> GetImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "image/{name}")] HttpRequest req,
            string name,
            [Blob("original/{name}", FileAccess.Read, Connection = "AzureWebJobsStorage")] CloudBlockBlob originalImage,
            [Blob("image-data/{name}.txt", FileAccess.Read, Connection = "AzureWebJobsStorage")] CloudBlockBlob imageData,
            ILogger log)
        {
            var accessPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var token = originalImage.GetSharedAccessSignature(accessPolicy);
            var sasUri = originalImage.Uri + token;
            var dataString = await imageData.DownloadTextAsync();

            var resultObject = new JObject(
                new JProperty("sourceUri", sasUri),
                new JProperty("data", JObject.Parse(dataString))
            );

            return new ObjectResult(resultObject)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
