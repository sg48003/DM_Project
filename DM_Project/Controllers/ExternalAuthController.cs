using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using DataAccess.Services;
using DM_Project.Helpers;
using DM_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace DM_Project.Controllers
{
    public class ExternalAuthController : Controller
    {
        private readonly UserService _userService;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _client = new HttpClient();

        public ExternalAuthController(UserService userService, IOptions<AppSettings> appSettings/*, JwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions*/)
        {
            _appSettings = appSettings.Value;
            _userService = userService;
        }

        [Route("api/externalauth/facebooklogin/")]
        [HttpPost]
        public async Task<IActionResult> FacebookLogin(string accessToken)
        {
            //korisnik mora biti tester da bi radilo, to se može postaviti na https://developers.facebook.com/ 
            //dodati povratnu vezu URL na stranicu https://developers.facebook.com/
            //treba ranije pozvati https://www.facebook.com/v2.11/dialog/oauth?&response_type=token&display=popup&client_id={_appSettings.FacebookAppId}&display=popup&redirect_uri={URL_ZA_POVRATNU_VEZU}&scope=email,user_birthday,user_friends
            //proslijediti ovoj metodi access token koji se dobije pozivanjem gornjeg url-a

            // 1.generate an app access token
            var appAccessTokenResponse =
                await _client.GetStringAsync(
                    $"https://graph.facebook.com/oauth/access_token?client_id={_appSettings.FacebookAppId}&client_secret={_appSettings.FacebookAppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);

            // 2. validate the user access token
            var userAccessTokenValidationResponse = await _client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation =
                JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                return BadRequest("Invalid facebook token.");
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await _client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,birthday,picture,friends&access_token={accessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            var facebookFriends = new List<FacebookFriend>();
            foreach (var item in userInfo.Friends.Data)
            {
                var friend = new FacebookFriend()
                {
                    FacebookId = item.Id,
                    Name = item.Name
                };
                facebookFriends.Add(friend);
            }

            // 4. ready to create the local user account (if necessary) and jwt
            var user = _userService.GetByEmail(userInfo.Email);

            if (user == null)
            {
                var userIn = new User()
                {
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    Username = userInfo.Email,
                    Email = userInfo.Email,
                    DateOfBirth = DateTime.Parse(userInfo.DateOfBirth, CultureInfo.InvariantCulture),
                    Image = userInfo.Picture.Data.Url,
                    FacebookId = userInfo.Id,
                    FacebookFriends = facebookFriends
                };
               
                _userService.RegisterFacebook(userIn);
                user = userIn;
            }
            else
            {
                var updateFlag = false;

                if (user.FacebookId == null)
                {
                    user.FacebookId = userInfo.Id;
                    updateFlag = true;
                }

                foreach (var item in facebookFriends)
                {
                    if (user.FacebookFriends.Contains(item) == false)
                    {
                        user.FacebookFriends = facebookFriends;
                        updateFlag = true;
                        break;
                    }
                }

                if (updateFlag)
                {
                    _userService.UpdateFacebookUser(user.Id, user);
                }                
            }

            //var jwt = await Tokens.GenerateJwt(_jwtFactory.GenerateClaimsIdentity(localUser.Username, localUser.Id.ToString()),
            //    _jwtFactory, localUser.Username, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });

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
    }
}
