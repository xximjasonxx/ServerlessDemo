using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessorDurable.Functions.Models
{
    public class ProcessImageInput
    {
        public string Name { get; set; }
        public string RawBlobData { get; set; }
    }
}
