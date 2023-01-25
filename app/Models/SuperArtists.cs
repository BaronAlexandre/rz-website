using SpotifyAPI.Web;

namespace app.Models
{
	public class SuperArtists : FullArtist
	{
		public List<Album> Albums { get; set; }
	}
}
