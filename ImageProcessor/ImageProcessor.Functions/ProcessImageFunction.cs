using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Functions
{
    public class ProcessImageFunction
    {
        private IConfiguration _configuration;

        public ProcessImageFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("ResizeImage")]
        public async Task ResizeImage(
            [BlobTrigger("original/{name}", Connection = "AzureWebJobsStorage")] Stream incomingBlob,
            [Blob("resized/{name}", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream resizedImage)
        {
            using (var memStream = new MemoryStream())
            {
                IImageFormat format;
                using (var image = Image.Load(incomingBlob, out format))
                {
                    var newWidth = (int)Math.Round(image.Width * .25m);
                    var newHeight = (int)Math.Round(image.Height * .25m);

                    image.Mutate(c => c.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
                    await image.SaveAsync(resizedImage, format);
                }
            }
        }

        [FunctionName("AnalyzeImage")]
        public async Task AnalyzeImage(
            [BlobTrigger("original/{name}", Connection = "AzureWebJobsStorage")] Stream incomingBlob,
            [Blob("image-data/{name}.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] CloudBlockBlob dataBlob)
        {
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(_configuration["CognitiveServicesKey"]))
            {
                Endpoint = _configuration["CognitiveServicesEndpoint"]
            };

            var features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            var analysisResult = await client.AnalyzeImageInStreamAsync(incomingBlob, features);
            var jsonAnalysis = analysisResult.AsJsonObject().ToString();
            await dataBlob.UploadTextAsync(jsonAnalysis);
        }
    }
}
