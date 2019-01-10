using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess.Models;
using DataAccess.Services;
using DM_Project.Helpers;
using DM_Project.Models;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IMDBCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace DM_Project.Controllers
{
    [Authorize]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly MovieService _movieService;
        private readonly UserService _userService;
        private readonly AppSettings _appSettings;
        private readonly Imdb _imdb;
        private readonly TrackService _trackService;
        private readonly LastfmClient _musicClient;

        public UsersController(MovieService movieService, UserService userService, TrackService trackService, IOptions<AppSettings> appSettings)
        {
            _movieService = movieService;
            _userService = userService;
            _trackService = trackService;
            _appSettings = appSettings.Value;
            _imdb = new Imdb(appSettings.Value.ImdbApiKey);
            _musicClient = new LastfmClient(appSettings.Value.LastfmApiKey, appSettings.Value.LastfmApiSecret);
        }

        [AllowAnonymous]
        [Route("api/users/register")]
        [HttpPost]
        public IActionResult Register([FromBody] RegisterUserModel userIn)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var user = new User()
            {
                FirstName = userIn.FirstName,
                LastName = userIn.LastName,
                Username = userIn.Username,
                Email = userIn.Email,
                DateOfBirth = userIn.DateOfBirth,
                Image = userIn.Image

            };
            _userService.Register(user, userIn.Password);
            return CreatedAtAction("GetMovieCollection", new { id = user.Id }, user);

        }

        [AllowAnonymous]
        [Route("api/users/authenticate")]
        [HttpPost]
        public IActionResult Authenticate([FromBody] LoginUserModel userIn)
        {
            var user = _userService.Authenticate(userIn.Username, userIn.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }

        [Route("api/users/movies/add")]
        [HttpPost]
        public ActionResult AddMovieToCollection(string imbdId, string userId, string comment, decimal rating)
        {
            ImdbMovie movie = _imdb.GetMovieFromIdAsync(imbdId).Result;
            if (_movieService.Exists(movie.ImdbId) == false)
            {
                _movieService.Create(movie);
            }

            var movieCollection = _userService.AddMovieToCollection(ObjectId.Parse(userId), movie, comment, rating);

            return CreatedAtAction("GetMovieCollection", new { id = movieCollection.Id }, movieCollection);
        }

        [Route("api/users/movies/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<MovieCollectionInfo>> GetMovieCollection(string userId)
        {
            return _userService.GetMovieCollection(ObjectId.Parse(userId));
        }

        [Route("api/users/movies/collection")]
        [HttpPost]
        public ActionResult UpdateMovieCollection(List<MovieCollectionInfo> movies)
        {
            _userService.UpdateMovieCollection(movies);
            return Ok();
        }

        [Route("api/users/movies/collection")]
        [HttpDelete]
        public ActionResult DeleteFromMovieCollection(string movieCollectionId)
        {
            _userService.DeleteMovieFromCollection(movieCollectionId);
            return Ok();
        }

        [Route("api/users/friends/movies/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<MovieCollectionInfo>> GetFriendsMovieCollection(string userId)
        {
            var movieRecommendations = new List<MovieCollectionInfo>();

            var user = _userService.GetById(ObjectId.Parse(userId));
            foreach (var friend in user.FacebookFriends)
            {
                movieRecommendations.AddRange(_userService.GetFacebookFriendsMovieCollection(friend.FacebookId));
            }

            return movieRecommendations;
        }

        [Route("api/users/tracks/add")]
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> AddTrackToCollectionAsync( string trackName, string artistName, string userId, string comment, decimal rating)
        {

            var response = await _musicClient.Track.GetInfoAsync(trackName, artistName);
            Track newTrack = new Track();
            newTrack.Title = response.Content.Name;
            newTrack.FmId = response.Content.Id;
            newTrack.Album = response.Content.AlbumName;
            newTrack.Artist = response.Content.ArtistName;


            if (_trackService.Exists(newTrack.FmId) == false)
            {
                _trackService.Create(newTrack);
            }

            _userService.AddTrackToCollection(ObjectId.Parse(userId), newTrack, comment, rating);

            return CreatedAtAction("GetTrackCollection", new { id = newTrack.Id }, newTrack);
        }


        [Route("api/users/tracks/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<TrackCollectionInfo>> GetTrackCollection(string userId)
        {
            return _userService.GetTrackCollection(ObjectId.Parse(userId));
        }

        [Route("api/users/tracks/collection")]
        [HttpPost]
        public ActionResult UpdateTrackCollection(List<TrackCollectionInfo> tracks)
        {
            _userService.UpdateTrackCollection(tracks);
            return Ok();
        }

        [Route("api/users/tracks/collection")]
        [HttpDelete]
        public ActionResult DeleteFromTrackCollection(string trackCollectionId)
        {
            _userService.DeleteTrackFromCollection(trackCollectionId);
            return Ok();
        }

        [Route("api/users/friends/tracks/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<TrackCollectionInfo>> GetFriendsTrackCollection(string userId)
        {
            var tracksRecommendations = new List<TrackCollectionInfo>();

            var user = _userService.GetById(ObjectId.Parse(userId));
            foreach (var friend in user.FacebookFriends)
            {
                tracksRecommendations.AddRange(_userService.GetFacebookFriendsTrackCollection(friend.FacebookId));
            }

            return tracksRecommendations;
        }


    }
}