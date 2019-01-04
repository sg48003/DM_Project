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

        public bool Exists(string id)
        {
            return _movies.Find(movie => movie.ImdbId == id).Any();
        }
    }
}
