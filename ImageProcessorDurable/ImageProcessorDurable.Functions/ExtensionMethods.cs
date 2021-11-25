using System;
using System.IO;

namespace ImageProcessorDurable.Functions
{
    internal static class ExtensionMethods
    {
        public static string AsBase64String(this Stream stream)
        {
            byte[] byteArray;
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                byteArray = memStream.ToArray();
            }

            return Convert.ToBase64String(byteArray);
        }
    }
}
