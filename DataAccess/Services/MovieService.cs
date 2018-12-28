using System.Collections.Generic;
using DataAccess.Models;
using IMDBCore;
using MongoDB.Driver;

namespace DataAccess.Services
{
    public class MovieService
    {
        private readonly IMongoCollection<Movie> _movies;

        public MovieService()
        {
            var client = new MongoClient();
            var database = client.GetDatabase("DM_ProjectDB");
            _movies = database.GetCollection<Movie>("Movies");
        }

        public List<Movie> Get()
        {
            return _movies.Find(movie => true).ToList();
        }

        public Movie Get(string id)
        {
            return _movies.Find(movie => movie.ImdbId == id).FirstOrDefault();
        }

        public Movie Create(ImdbMovie movieIn)
        {
            var movie = new Movie()
            {
                Title = movieIn.Title,
                Year = movieIn.Year,
                Runtime = movieIn.RunTime,
                Genre = movieIn.Genre,
                Director = movieIn.Director,
                Writer = movieIn.Writer,
                Actors = movieIn.Actors,
                Plot = movieIn.Plot,
                Language = movieIn.Language,
                Awards = movieIn.Awards,
                Poster = movieIn.Poster,
                ImdbRating = movieIn.ImdbRating,
                TomatoRating = movieIn.TomatoRating,
                ImdbId = movieIn.ImdbId,
                Rated = movieIn.Rated,
                Production = movieIn.Production
            };
            _movies.InsertOne(movie);
            return movie;
        }

        public void Update(string id, Movie movieIn)
        {
            _movies.ReplaceOne(movie => movie.ImdbId == id, movieIn);
        }

        public void Remove(Movie movieIn)
        {
            _movies.DeleteOne(movie => movie.ImdbId == movieIn.ImdbId);
        }

        public void Remove(string id)
        {
            _movies.DeleteOne(movie => movie.ImdbId == id);
        }

        public bool Exists(string id)
        {
            return _movies.Find(movie => movie.ImdbId == id).Any();
        }
    }
}
