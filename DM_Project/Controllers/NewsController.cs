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
    public class NewsController : ControllerBase
    {   
        private readonly string NewsApiKey; 

        public NewsController(IOptions<AppSettings> appSettings)
        {
            NewsApiKey = appSettings.Value.NewsApiKey;
        }



        [Route("api/news/search")]
        [HttpGet]

        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<SearchNewsModel>>> NewsAsync(string searchInput)
        {
            //"https://newsapi.org/v2/everything?q=" + searchInput +"&from=2018-12-10&sortBy=publishedAt&apiKey=" + NewsApiKey
            string url = "https://newsapi.org/v2/everything?q=" + searchInput + "&from=2018-12-10&sortBy=publishedAt&apiKey=" + NewsApiKey;
            string jsonString;
            using (var httpClient = new HttpClient())
            {
                jsonString = await httpClient.GetStringAsync(url);

                
            }
            NewsObject json = JsonConvert.DeserializeObject<NewsObject>(jsonString);
            List<SearchNewsModel> list = new List<SearchNewsModel>();
            foreach (var element in json.articles)
            {
                SearchNewsModel news = new SearchNewsModel();

                news.Author = element.author;
                news.Content = element.content;
                news.Title = element.title;
                list.Add(news);
            }

            return list;


        }

 
    }

    
    }


