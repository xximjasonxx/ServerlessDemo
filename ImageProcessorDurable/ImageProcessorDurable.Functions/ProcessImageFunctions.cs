using Azure.Storage.Blobs;
using ImageProcessorDurable.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions
{
    public class ProcessImageFunctions
    {
        [FunctionName("SaveImage")]
        public async Task SaveImage(
            [ActivityTrigger] IDurableActivityContext context,
            [Blob("original", FileAccess.Write, Connection = "AzureWebJobsStorage")] BlobContainerClient container)
        {
            var input = context.GetInput<ProcessImageInput>();
            var byteData = Convert.FromBase64String(input.RawBlobData);

            using var memStream = new MemoryStream(byteData);
            var blobReference = container.GetBlobClient(input.Name);
            await blobReference.UploadAsync(memStream);
        }

        [FunctionName("ResizeImage")]
        public async Task ResizeImage(
            [ActivityTrigger] IDurableActivityContext context,
            [Blob("resized", FileAccess.Write, Connection = "AzureWebJobsStorage")] BlobContainerClient container)
        {
            var input = context.GetInput<ProcessImageInput>();
            var byteData = Convert.FromBase64String(input.RawBlobData);

            // prepare our stream
            IImageFormat format;
            using var byteDataMemStream = new MemoryStream(byteData);
            using var image = Image.Load(byteDataMemStream, out format);
            using var outStream = new MemoryStream();

            var newWidth = (int)Math.Round(image.Width * .25m);
            var newHeight = (int)Math.Round(image.Height * .25m);
            image.Mutate(c => c.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
            await image.SaveAsync(outStream, format);

            outStream.Position = 0;
            var blobReference = container.GetBlobClient(input.Name);
            await blobReference.UploadAsync(outStream);
        }
    }
}
