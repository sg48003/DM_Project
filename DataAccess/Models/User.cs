using System;
using MongoDB.Bson;

namespace DataAccess.Models
{
    //TODO:
    public class User
    {
        public ObjectId Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long? FacebookId { get; set; }
        public string Image { get; set; } //možda byte[] ???
    }
}
