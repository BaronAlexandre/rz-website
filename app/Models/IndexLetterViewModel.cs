using SpotifyAPI.Web;

namespace app.Models
{
    public class IndexLetterViewModel
	{
        public List<PaginationLetter> PaginationLetters { get; set; }
        public List<SuperArtists> Artists { get; set; }
    }
}