using MongoDB.Bson;

namespace DataAccess.Models
{
    public class MovieCollection
    {
        public ObjectId Id { get; set; }
        public string MovieId { get; set; }
        public ObjectId UserId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
    }
}
