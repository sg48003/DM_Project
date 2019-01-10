using DataAccess.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Services
{
    public class TrackService
    {
        private readonly IMongoCollection<Track> _tracks;
        public TrackService()
        {
            var client = new MongoClient();
            var database = client.GetDatabase("DM_ProjectDB");
            _tracks = database.GetCollection<Track>("Tracks");
        }

        public Track Create(Track newTrack)
        {
            var track = new Track()
            {
                Title = newTrack.Title,
                Album = newTrack.Album,
                Artist = newTrack.Artist,
                FmId = newTrack.FmId
 
            };
            _tracks.InsertOne(track);
            return track;
        }

        public bool Exists(int id)
        {
            return _tracks.Find(track => track.FmId == id).Any();
        }
    }
}
