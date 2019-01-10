using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Track
    {
        public ObjectId Id { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string FmId { get; set; }

    }
}
