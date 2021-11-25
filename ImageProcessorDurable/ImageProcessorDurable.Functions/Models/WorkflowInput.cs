using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions.Models
{
    public class WorkflowInput
    {
        public string BlobData { get; init; }
        public string OriginalName { get; init; }
    }
}
