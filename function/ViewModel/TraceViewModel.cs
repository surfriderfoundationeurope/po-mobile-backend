using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.ViewModel
{
    public class TraceViewModel
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public string move { get; set; }
        public string bank { get; set; }
        public string trackingMode { get; set; }
        public File[] files { get; set; }
        public Trash[] trashes { get; set; }
        public Position[] positions { get; set; }
        public string comment { get; set; }
        public bool isStarted { get; set; }
        public bool isFinished { get; set; }
        public bool isSynced { get; set; }
    }

    public class File
    {
        public DateTime date { get; set; }
        public string filename { get; set; }
        public bool isUploaded { get; set; }
        public string fullName { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public string id { get; set; }
    }

    public class Trash
    {
        public DateTime date { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public string name { get; set; }
    }

    public class Position
    {
        public float lat { get; set; }
        public float lng { get; set; }
        public DateTime date { get; set; }
    }
}
