using SpotifyAPI.Web;

namespace app.Models
{
    public class Album : SimpleAlbum
    {
        public List<Track> Tracks { get; set; }
        public SuperArtists Artist { get; set; }
    }
}
