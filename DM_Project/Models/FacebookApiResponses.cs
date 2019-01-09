using System.Collections.Generic;
using Newtonsoft.Json;

namespace DM_Project.Models
{
    internal class String
    {
        public long Id { get; set; }
        public string Email { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("birthday")]
        public string DateOfBirth { get; set; }
        public FacebookPictureData Picture { get; set; }
        public FacebookFriends Friends { get; set; }
    }

    internal class FacebookPictureData
    {
        public FacebookPicture Data { get; set; }
    }

    internal class FacebookPicture
    {
        public int Height { get; set; }
        public int Width { get; set; }
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }
        public string Url { get; set; }
    }

    internal class FacebookUserAccessTokenData
    {
        [JsonProperty("app_id")]
        public long AppId { get; set; }
        public string Type { get; set; }
        public string Application { get; set; }
        [JsonProperty("expires_at")]
        public long ExpiresAt { get; set; }
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }
        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }

    internal class FacebookUserAccessTokenValidation
    {
        public FacebookUserAccessTokenData Data { get; set; }
    }

    internal class FacebookFriends
    {
        public List<FacebookFriendsData> Data { get; set; }
        public Paging Paging { get; set; }
        public Summary Summary { get; set; }
    }

    internal class FacebookFriendsData
    {
        public string Name { get; set; }
        public long Id { get; set; }
    }

    internal class Cursors
    {
        public string Before { get; set; }
        public string After { get; set; }
    }

    internal class Paging
    {
        public Cursors Cursors { get; set; }
    }

    internal class Summary
    {
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }

    internal class FacebookAppAccessToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
