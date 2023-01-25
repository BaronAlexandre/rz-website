using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace app.Models
{
	public class Track
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int DurationMs { get; set; }
		[JsonProperty("Text")]
		public string TextTool { get; set; }
		public List<TrackElement> TextItem { get; set; }
		public Album Album { get; set; }
		public string Timeline { get; set; }
		public List<SongTimeline> Repartition { get; set; }
		public TrackAudioFeatures Stats { get; set; }
	}
}
