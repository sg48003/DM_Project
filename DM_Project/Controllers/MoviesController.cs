using System.Collections.Generic;
using System.Linq;
using DM_Project.Helpers;
using IMDBCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using SearchMovie = TMDbLib.Objects.Search.SearchMovie;

namespace DM_Project.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {   
        private readonly IOptions<AppSettings> _appSettings;
        private readonly TMDbClient _client;
        private readonly Imdb _imdb;

        public MoviesController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _client = new TMDbClient(appSettings.Value.TMDbApiKey);
            _imdb = new Imdb(appSettings.Value.ImdbApiKey);
        }

        [Route("api/movie/search")]
        [HttpGet]
        public ActionResult<IEnumerable<Models.SearchMovie>> Search(string name, int? year = null, int? genre = null)
        {
            SearchContainer<SearchMovie> searchMovies = _client.SearchMovieAsync(name, 1, false, year ?? 0).Result;
            var genres = _client.GetMovieGenresAsync().Result;
            IEnumerable<SearchMovie> filteredMovies = genre != null ? searchMovies.Results.Where(x => x.GenreIds.Contains((int)genre)).ToList() : searchMovies.Results.ToList();

            return filteredMovies.Select(movie => new Models.SearchMovie
            {
                Title = movie.Title,
                ReleaseDate = movie.ReleaseDate,
                Genres = string.Join(",", genres.FindAll(x => movie.GenreIds.Contains(x.Id)).Select(x => x.Name)),
                Id = movie.Id,
                PosterPath = "http://image.tmdb.org/t/p/w185//" + movie.PosterPath,
                Language = movie.OriginalLanguage,
                BackdropPath = "http://image.tmdb.org/t/p/w185//" + movie.BackdropPath,
                Overview = movie.Overview
            })
                .ToList();
        }

        [Route("api/movies/popular")]
        [HttpGet]
        public ActionResult<IEnumerable<Models.SearchMovie>> Popular()
        {
            IEnumerable<SearchMovie> popularMovies = _client.GetMoviePopularListAsync().Result.Results;
            var genres = _client.GetMovieGenresAsync().Result;

            return popularMovies.Select(movie => new Models.SearchMovie
            {
                Title = movie.Title,
                ReleaseDate = movie.ReleaseDate,
                Genres = string.Join(",", genres.FindAll(x => movie.GenreIds.Contains(x.Id)).Select(x => x.Name)),
                Id = movie.Id,
                PosterPath = "http://image.tmdb.org/t/p/w185//" + movie.PosterPath,
                Language = movie.OriginalLanguage,
                BackdropPath = "http://image.tmdb.org/t/p/w185//" + movie.BackdropPath,
                Overview = movie.Overview
            })
                .ToList();
        }

        [Route("api/movies/details")]
        [HttpGet]
        public ActionResult<ImdbMovie> Details(int id)
        {
            string imbdId = _client.GetMovieAsync(id).Result.ImdbId;
            ImdbMovie movie = _imdb.GetMovieFromIdAsync(imbdId).Result;

            return movie;
        }

        [Route("api/movies/recommendations")]
        [HttpGet]
        public ActionResult<IEnumerable<Models.SearchMovie>> Recommendations(int id)
        {
            List<SearchMovie> recommendedMovies = _client.GetMovieRecommendationsAsync(id).Result.Results;
            var genres = _client.GetMovieGenresAsync().Result;
            return recommendedMovies.Select(movie => new Models.SearchMovie
                {
                    Title = movie.Title,
                    ReleaseDate = movie.ReleaseDate,
                    Genres = string.Join(",", genres.FindAll(x => movie.GenreIds.Contains(x.Id)).Select(x => x.Name)),
                    Id = movie.Id,
                    PosterPath = "http://image.tmdb.org/t/p/w185//" + movie.PosterPath,
                    Language = movie.OriginalLanguage,
                    BackdropPath = "http://image.tmdb.org/t/p/w185//" + movie.BackdropPath,
                    Overview = movie.Overview
                })
                .ToList();
        }

    }
}
