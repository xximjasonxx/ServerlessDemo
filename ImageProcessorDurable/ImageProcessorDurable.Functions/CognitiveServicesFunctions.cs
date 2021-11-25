using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions
{
    public class CognitiveServicesFunctions
    {
        [FunctionName("GetImageIsProhibited")]
        public async Task<bool> GetImageIsProhibited(
            [ActivityTrigger]IDurableActivityContext context)
        {
            var byteArray = Convert.FromBase64String(context.GetInput<string>());
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials("41e0aa0504ad413499994cfa7aeae616"))
            {
                Endpoint = "https://eastus2.api.cognitive.microsoft.com/"
            };

            using (var memStream = new MemoryStream(byteArray))
            {
                var result = await client.AnalyzeImageInStreamAsync(memStream, new List<VisualFeatureTypes?> { VisualFeatureTypes.Adult });
                return result.Adult.IsAdultContent == true
                    || result.Adult.IsGoryContent == true
                    || result.Adult.IsRacyContent == true;
            }
        }
    }
}
