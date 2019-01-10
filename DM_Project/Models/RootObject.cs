using System;
using System.Collections.Generic;

namespace DM_Project.Models
{
    public class ExternalIds
    {
        public string spotify { get; set; }
    }

    public class Artist
    {
        public string _id { get; set; }
        public ExternalIds external_ids { get; set; }
        public string name { get; set; }
    }

    public class ExternalIds2
    {
        public string spotify { get; set; }
    }

    public class Result
    {
        public string _id { get; set; }
        public List<Artist> artists { get; set; }
        public string audio_url { get; set; }
        public ExternalIds2 external_ids { get; set; }
        public string title { get; set; }
    }

    public class RootObject
    {
        public int num_results { get; set; }
        public List<Result> results { get; set; }
        public bool success { get; set; }
    }
}
