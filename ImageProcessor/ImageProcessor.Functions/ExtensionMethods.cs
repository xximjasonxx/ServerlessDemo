using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ImageProcessor.Functions
{
    public static class ExtensionMethods
    {
        public static JObject AsJsonObject(this ImageAnalysis analysisResult)
        {
            var newObject = new JObject(
                new JProperty("adult", analysisResult.Adult?.AsJsonObject()),
                new JProperty("brands", analysisResult.Brands?.AsJsonObject()),
                new JProperty("categories", analysisResult.Categories?.AsJsonObject()),
                new JProperty("color", analysisResult.Color?.AsJsonObject()),
                new JProperty("description", analysisResult.Description?.AsJsonObject()),
                new JProperty("faces", analysisResult.Faces?.AsJsonObject()),
                new JProperty("imageType", analysisResult.ImageType?.AsJsonObject()),
                new JProperty("metadata", analysisResult.Metadata?.AsJsonObject()),
                new JProperty("modelVersion", analysisResult.ModelVersion?.ToString()),
                new JProperty("objects", analysisResult.Objects?.AsJsonObject()),
                new JProperty("requestId", analysisResult.RequestId?.ToString()),
                new JProperty("tags", analysisResult.Tags?.AsJsonObject())
            );

            return newObject;
        }

        private static JObject AsJsonObject<TObject>(this TObject infoBlock)
        {
            var properties = new List<JProperty>(infoBlock.GetType().GetProperties()
                .Where(x => x.PropertyType.IsGenericType == false)
                .Select(x => new JProperty(x.Name.ToLower(), x.GetValue(infoBlock)?.GetValue()))
            );
            
            properties.AddRange(infoBlock.GetType().GetProperties()
                .Where(x => x.PropertyType.IsGenericType && typeof(IList<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition()))
                .Select(x => new JProperty(x.Name.ToLower(), ((IEnumerable)x.GetValue(infoBlock))?.AsList()))
            );   

            return new JObject(properties);
        }

        private static JArray AsList(this IEnumerable enumeration)
        {
            var propertyList = new List<JToken>();
            foreach (var value in enumeration)
            {
                if (value.GetType().IsValueType || value.GetType() == typeof(string))
                {
                    propertyList.Add(new JValue(value.ToString()));
                }
                else
                {
                    propertyList.Add(value.AsJsonObject());
                }
            }

            return new JArray(propertyList.Select(x => x));
        }

        private static JToken GetValue(this object objectValue)
        {
            var objectType = objectValue.GetType();
            if (objectType.IsValueType || objectType == typeof(string))
            {
                return new JValue(objectValue.ToString());
            }

            return objectValue.AsJsonObject();
        }

        private static JObject AsJsonObject<TObject>(this IList<TObject> items)
        {
            return new JObject(
                new JProperty("items", new JArray(
                    items.Select(x => x.AsJsonObject()))
                ));
        }
    }
}
