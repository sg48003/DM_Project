using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess.Models;
using DataAccess.Services;
using DM_Project.Helpers;
using DM_Project.Models;
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

        public UsersController(MovieService movieService, UserService userService, IOptions<AppSettings> appSettings)
        {
            _movieService = movieService;
            _userService = userService;
            _appSettings = appSettings.Value;
            _imdb = new Imdb(appSettings.Value.ImdbApiKey);
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

            _userService.AddMovieToCollection(ObjectId.Parse(userId), movie, comment, rating);

            return CreatedAtAction("GetMovieCollection", new { id = movie.ImdbId }, movie);
        }

        [Route("api/users/movies/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<Movie>> GetMovieCollection(string userId)
        {
            return _userService.GetMovieCollection(ObjectId.Parse(userId));
        }

        [Route("api/users/friends/movies/collection")]
        [HttpGet]
        public ActionResult<IEnumerable<Movie>> GetFriendsMovieCollection(string userId)
        {
            var movieRecommendations = new List<Movie>();

            var user = _userService.GetById(ObjectId.Parse(userId));
            foreach (var friend in user.FacebookFriends)
            {
                movieRecommendations.AddRange(_userService.GetFacebookFriendsMovieCollection(friend.FacebookId));
            }

            return movieRecommendations;
        }

    }
}