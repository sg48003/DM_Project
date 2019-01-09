using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMongoCollection<TrackCollection> _trackCollections;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Movie> _movies;
        private readonly IMongoCollection<Track> _tracks;

        public UserService()
        {
            var client = new MongoClient();
            var database = client.GetDatabase("DM_ProjectDB");
            _movieCollections = database.GetCollection<MovieCollection>("MovieCollections");
            _trackCollections = database.GetCollection<TrackCollection>("TrackCollections");
            _users = database.GetCollection<User>("Users");
            _movies = database.GetCollection<Movie>("Movies");
            _tracks = database.GetCollection<Track>("Tracks");
        }

        public User GetById(ObjectId id)
        {
            return _users.Find(x => x.Id == id).SingleOrDefault();
        }

        public User GetByEmail(string email)
        {
            return _users.Find(x => x.Email == email).SingleOrDefault();
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

        public User RegisterFacebook(User userIn)
        {
            if (_users.Find(user => user.Username == userIn.Username).Any())
            {
                throw new Exception("Username \"" + userIn.Username + "\" is already taken");
            }
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

        public void UpdateFacebookUser(ObjectId id, User userIn)
        {
            _users.ReplaceOne(user => user.Id == id, userIn);
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

        public List<MovieCollectionInfo> GetMovieCollection(ObjectId userId)
        {
            var movieCollectionInfo = new List<MovieCollectionInfo>();

            List<MovieCollection> movieCollection = _movieCollections.Find(collection => collection.UserId == userId).ToList();
            foreach (var item in movieCollection)
            {
                var movie = _movies.Find(x => x.Id == ObjectId.Parse(item.MovieId)).SingleOrDefault();
                if (movie != null)
                {
                    var newMovieInfo = new MovieCollectionInfo()
                    {
                        Movie = movie,
                        MovieCollection = item
                    };
                    movieCollectionInfo.Add(newMovieInfo);
                }
            }

            return movieCollectionInfo;
        }

        public void UpdateMovieCollection(List<MovieCollectionInfo> movieCollectionInfoIn)
        {
            foreach (var item in movieCollectionInfoIn)
            {
                _movieCollections.ReplaceOne(movie => movie.Id == item.MovieCollection.Id, item.MovieCollection);
            }
        }

        public void DeleteMovieFromCollection(string movieCollectionId)
        {
            _movieCollections.DeleteOne(x => x.Id == ObjectId.Parse(movieCollectionId));
        }
        

        public List<MovieCollectionInfo> GetFacebookFriendsMovieCollection(long facebookId)
        {
            var movieCollectionInfo = new List<MovieCollectionInfo>();
            var user =_users.Find(x => x.FacebookId == facebookId).SingleOrDefault();

            List<MovieCollection> movieCollection = _movieCollections.Find(collection => collection.UserId == user.Id).ToList();
            foreach (var item in movieCollection)
            {
                var movie = _movies.Find(x => x.Id == ObjectId.Parse(item.MovieId)).SingleOrDefault();
                if (movie != null)
                {
                    var newMovieInfo = new MovieCollectionInfo()
                    {
                        Movie = movie,
                        MovieCollection = item
                    };
                    movieCollectionInfo.Add(newMovieInfo);
                }
            }

            return movieCollectionInfo;
        }

        public List<TrackCollectionInfo> GetTrackCollection(ObjectId userId)
        {
            var trackCollectionInfo = new List<TrackCollectionInfo>();

            List<TrackCollection> trackCollection = _trackCollections.Find(collection => collection.UserId == userId).ToList();
            foreach (var item in trackCollection)
            {
                var track = _tracks.Find(x => x.Id == item.TrackId);
                if (track != null)
                {
                    var newTrackInfo = new TrackCollectionInfo()
                    {
                        Track = (Track) track,
                        TrackCollection = item
                    };
                    trackCollectionInfo.Add(newTrackInfo);
                }
            }

            return trackCollectionInfo;
        }

        public TrackCollection AddTrackToCollection(ObjectId userId, Track track, string comment, decimal rating)
        {
            var trackCollection = new TrackCollection()
            {
                TrackId = track.Id,
                UserId = userId,
                Rating = rating,
                Comment = comment
            };
            _trackCollections.InsertOne(trackCollection);
            return trackCollection;
        }

        public void UpdateTrackCollection(List<TrackCollectionInfo> trackCollectionInfoIn)
        {
            foreach (var item in trackCollectionInfoIn)
            {
                _trackCollections.ReplaceOne(movie => movie.Id == item.TrackCollection.Id, item.TrackCollection);
            }
        }

        public void DeleteTrackFromCollection(string trackCollectionId)
        {
            _trackCollections.DeleteOne(x => x.Id == ObjectId.Parse(trackCollectionId));
        }


        public List<TrackCollectionInfo> GetFacebookFriendsTrackCollection(long facebookId)
        {
            var trackCollectionInfo = new List<TrackCollectionInfo>();
            var user = _users.Find(x => x.FacebookId == facebookId).SingleOrDefault();

            List<TrackCollection> trackCollection = _trackCollections.Find(collection => collection.UserId == user.Id).ToList();
            foreach (var item in trackCollection)
            {
                var track = _tracks.Find(x => x.Id == item.TrackId).SingleOrDefault();
                if (track != null)
                {
                    var newTrackInfo = new TrackCollectionInfo()
                    {
                        Track = track,
                        TrackCollection = item
                    };
                    trackCollectionInfo.Add(newTrackInfo);
                }
            }

            return trackCollectionInfo;
        }
    }
}
