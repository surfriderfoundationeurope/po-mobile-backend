using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class Trace
    {
        public Guid Id { get; set; }
        public string Locomotion { get; set; }
        public bool IsAiDriven { get; set; }
        public Guid UserId { get; set; }
        public string Riverside { get; set; }
        public DateTime CapturedOn { get; set; }
    }
}
