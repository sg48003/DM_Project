using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DataAccess.Helpers;
using DataAccess.Models;
using IMDBCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccess.Services
{
    public class UserService
    {
        private readonly IMongoCollection<MovieCollection> _movieCollections;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Movie> _movies;

        public UserService()
        {
            var client = new MongoClient();
            var database = client.GetDatabase("DM_ProjectDB");
            _movieCollections = database.GetCollection<MovieCollection>("MovieCollections");
            _users = database.GetCollection<User>("Users");
            _movies = database.GetCollection<Movie>("Movies");
        }

        public User GetById(ObjectId id)
        {
            return _users.Find(x => x.Id == id).SingleOrDefault();
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _users.Find(x => x.Username == username).SingleOrDefault();

            if (user == null)
            {
                return null;
            }

            if (PasswordHash.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt) == false)
            {
                return null;
            }

            // authentication successful
            return user;
        }

        public User Register(User userIn, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Password is required");
            }
            if (_users.Find(user => user.Username == userIn.Username).Any())
            {
                throw new Exception("Username \"" + userIn.Username + "\" is already taken");
            }

            PasswordHash.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

            userIn.PasswordHash = passwordHash;
            userIn.PasswordSalt = passwordSalt;

            _users.InsertOne(userIn);
            return userIn;
        }

        public void Update(User userParam, string password = null)
        {
            var user = _users.Find(x => x.Id == userParam.Id).SingleOrDefault();

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (userParam.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (_users.Find(x => x.Username == userParam.Username).Any())
                {
                    throw new Exception("Username " + userParam.Username + " is already taken");
                }

            }

            // update user properties
            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;
            user.Username = userParam.Username;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                PasswordHash.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _users.ReplaceOne(x => x.Id == user.Id, user);
        }

        public void Update(ObjectId id, User userIn)
        {
            _users.ReplaceOne(user => user.Id == id, userIn);
        }

        public void Remove(User userIn)
        {
            _users.DeleteOne(user => user.Id == userIn.Id);
        }

        public void Remove(ObjectId id)
        {
            _users.DeleteOne(user => user.Id == id);
        }

        public MovieCollection AddMovieToCollection(ObjectId userId, ImdbMovie movieIn, string comment, decimal rating)
        {
            var movieCollection = new MovieCollection()
            {
                MovieId = movieIn.ImdbId,
                UserId = userId,
                Rating = rating,
                Comment = comment
            };
            _movieCollections.InsertOne(movieCollection);
            return movieCollection;
        }

        public List<Movie> GetMovieCollection(ObjectId userId)
        {
            List<MovieCollection> movieCollection = _movieCollections.Find(collection => collection.UserId == userId).ToList();
            List<string> movieIds = movieCollection.Select(x => x.MovieId).ToList();

            var filterDef = new FilterDefinitionBuilder<Movie>();
            var filter = filterDef.In(x => x.ImdbId, movieIds);

            return _movies.Find(filter).ToList();
        }


    }
}
