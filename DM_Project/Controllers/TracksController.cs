using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DM_Project.Helpers;
using DM_Project.Models;
using IF.Lastfm.Core.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;



namespace DM_Project.Controllers
{
    [ApiController]
    public class TracksController : ControllerBase
    {   
        private readonly LastfmClient _client;
        private readonly string GalibooKey; 

        public TracksController(IOptions<AppSettings> appSettings)
        {
            _client = new LastfmClient(appSettings.Value.LastfmApiKey, appSettings.Value.LastfmApiSecret);
            GalibooKey = appSettings.Value.GalibooApiKey;
        }

        [Route("api/track/search")]
        [HttpGet]
        public ActionResult<IEnumerable<SearchTrackModel>> Search(string name)
        {
            var trackResult = _client.Track.SearchAsync(name).Result;
            
            return trackResult.Select(track => new SearchTrackModel()
            {

                Title = track.Name,
                Artist = track.ArtistName,
                Album = track.AlbumName
            }).ToList();

        }



        [Route("api/tracks/top10")]
        [HttpGet]
        public ActionResult<IEnumerable<SearchTrackModel>> TopTracks()
        {
            var popularTracks = _client.Chart.GetTopTracksAsync(page:1,itemsPerPage:10).Result;
            return popularTracks.Select(track => new SearchTrackModel()
            {
                Title = track.Name,
                Artist = track.ArtistName,
                Album = track.AlbumName,
                

            }).ToList();

     
        }


        [Route("api/tracks/recommendations")]
        [HttpGet]
        public ActionResult<IEnumerable<SearchTrackModel>> Recommendations(string name)
        {
            var tracks = _client.Track.SearchAsync(name).Result;
            var trackNames = tracks.Select(track => track.Name);
            var artists = tracks.Select(track2 => track2.ArtistName);
            string artist = artists.First();
            string trackName = trackNames.First();

            var similarTracks = _client.Track.GetSimilarAsync(trackName, artist).Result;
   
            return similarTracks.Select(track => new SearchTrackModel()
            {
                Title = track.Name,
                Artist = track.ArtistName,
                Album = track.AlbumName,


            }).ToList();


        }

        [Route("api/tracks/emotions")]
        [HttpGet]

        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<SearchTrack>>> EmotionsAsync(string emotion)
        {
 
            string url = "https://secure.galiboo.com/api/discover/tracks/smart_search/?token=" + GalibooKey + "&q=" + emotion;
            string[] array = new string[50];
            string jsonString;
            using (var httpClient = new HttpClient())
            {
                jsonString = await httpClient.GetStringAsync(url);

                // Now parse with JSON.Net
            }
            RootObject json = JsonConvert.DeserializeObject<RootObject>(jsonString);
            List<SearchTrack> list = new List<SearchTrack>();
            foreach (var element in json.results)
            {
                SearchTrack track = new SearchTrack();
                track.Title = element.title;
                string artist = "";
                foreach(var artistEl in element.artists)
                {

                    artist = artistEl.name;
                    if(element.artists.Count == 1)
                    {
                        break;
                    }  else
                    {
                        artist = artist + ", ";
                    }

                }
                track.Artist = artist;
                list.Add(track);
            }

            return list;
            /*
            string withoutLeftBrac = jsonString.Replace(@"{", string.Empty);
            string withoutRightBrac = withoutLeftBrac.Replace(@"}", string.Empty);
            string withoutComas = withoutRightBrac.Replace(@",", string.Empty);
            string withoutLeft = withoutComas.Replace(@"[", string.Empty);
            string withoutRight = withoutLeft.Replace(@"]", string.Empty);
            array = withoutRight.Split(":");
            int[] titles = new int[100];
            int counter = 0;
            int[] autors = new int[100];
            int counter2 = 0;
            for (int i = 0; i < array.Length;i++)
            {
                if (array.ElementAt(i).Equals("title"))
                {
                    titles[counter] = i + 1;
                    counter++;
                }

                if (array.ElementAt(i).Equals("name"))
                {
                    autors[counter2] = i + 1;
                    counter++;
                }

            }
            List<SearchTrack> list = new List<SearchTrack>();
            for(int j = 0; j < counter; j++)
            {
                SearchTrack track = new SearchTrack()
                {
                    Title = array.ElementAt(titles[j]),
                    Artist = array.ElementAt(autors[j])
                };

                list.Add(track);
            }

            return list;*/


        }

       /* public  int[] FindAllIndexOf<T>(T[] array, Predicate<T> match)
        {
            return array.Select((value, index) => match(value) ? index : -1)
                    .Where(index => index != -1).ToArray();
        }*/
    }

    
    }


