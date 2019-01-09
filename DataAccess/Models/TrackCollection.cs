using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class TrackCollection
    {
        public ObjectId Id { get; set; }
        public ObjectId TrackId { get; set; }
        public ObjectId UserId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
    }
}
