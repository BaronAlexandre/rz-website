using SpotifyAPI.Web;

namespace app.Models
{
    public class HomeViewModel
    {
        public List<SuperArtists> Artists { get; set; }
        public Pagination Pagination{ get; set; }
    }
}