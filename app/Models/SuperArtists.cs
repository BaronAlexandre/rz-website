using SpotifyAPI.Web;

namespace app.Models
{
    public class SuperArtists : FullArtist
    {
        public Paging<SimpleAlbum> Albums { get; set; }
    }
}
