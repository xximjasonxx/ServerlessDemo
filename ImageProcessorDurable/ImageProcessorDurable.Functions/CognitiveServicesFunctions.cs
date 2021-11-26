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
        private readonly IConfiguration _configuration;

        public CognitiveServicesFunctions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetImageIsProhibited")]
        public async Task<bool> GetImageIsProhibited(
            [ActivityTrigger]IDurableActivityContext context)
        {
            var byteArray = Convert.FromBase64String(context.GetInput<string>());
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration["CognitiveServicesApi"]))
            {
                Endpoint = _configuration["CognitiveServicesEndpoint"]
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
