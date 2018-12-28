using System.Collections.Generic;
using System.Linq;
using IMDBCore;
using Microsoft.AspNetCore.Mvc;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using SearchMovie = TMDbLib.Objects.Search.SearchMovie;

namespace DM_Project.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {
        //TODO: api key u config
        private readonly TMDbClient client = new TMDbClient("e3dac99f309c1f84e4db4fc22e756340");
        private readonly Imdb imdb = new Imdb("e15da83d");

        [Route("api/movie/search")]
        [HttpGet]
        public ActionResult<IEnumerable<Models.SearchMovie>> Search(string name, int? year = null, int? genre = null)
        {
            SearchContainer<SearchMovie> searchMovies = client.SearchMovieAsync(name, 1, false, year ?? 0).Result;
            var genres = client.GetMovieGenresAsync().Result;
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
            IEnumerable<SearchMovie> popularMovies = client.GetMoviePopularListAsync().Result.Results;
            var genres = client.GetMovieGenresAsync().Result;

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
            string imbdId = client.GetMovieAsync(id).Result.ImdbId;
            ImdbMovie movie = imdb.GetMovieFromIdAsync(imbdId).Result;

            return movie;
        }

    }
}
