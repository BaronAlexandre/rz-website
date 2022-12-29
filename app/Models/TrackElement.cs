namespace app.Models
{
	public class TrackElement
	{
		public string Title { get; set; }
		public List<string> Lines { get; set; }
		public double Percentage { get; set; }
		public TrackElementType Type { get; set; }
	}
}
