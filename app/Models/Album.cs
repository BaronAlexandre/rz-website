using SpotifyAPI.Web;

namespace app.Models
{
    public class Album
    {
        public string Id { get; set; }
        public List<Track> Tracks { get; set; }
        public SuperArtists Artist { get; set; }
    }
}
