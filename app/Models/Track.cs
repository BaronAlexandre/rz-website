﻿using SpotifyAPI.Web;

namespace app.Models
{
    public class Track
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DurationMs { get; set; }
        public string Text { get; set; }
        public Album Album { get; set; }
    }
}
