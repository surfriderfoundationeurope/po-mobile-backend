using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surfrider.PlasticOrigins.Backend.Mobile.ViewModel
{
    internal class TraceAttachmentUploadDetailsViewModel
    {
        public string TraceId { get; set; }
        public string UploadUrl { get; set; }
        public Dictionary<string, string> HttpHeaders { get; set; } = new Dictionary<string, string>();

    }
}
