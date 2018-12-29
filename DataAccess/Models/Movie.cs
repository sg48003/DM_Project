﻿using MongoDB.Bson;

namespace DataAccess.Models
{
    public class Movie
    {
        public ObjectId Id { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Language { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public string ImdbRating { get; set; }
        public string TomatoRating { get; set; }
        public string ImdbId { get; set; }
        public string Rated { get; set; }
        public string Production { get; set; }
    }
}