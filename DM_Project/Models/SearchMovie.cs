using System;

namespace DM_Project.Models
{
    public class SearchMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public string Language { get; set; }
        public string Genres { get; set; }
        public string BackdropPath { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
