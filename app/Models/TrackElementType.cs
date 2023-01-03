using System.ComponentModel;

namespace app.Models
{
	public enum TrackElementType
	{
		[Description("Autre")]
		None = 0,
		[Description("Couplet")]
		Verse = 1,
		[Description("Refrain")]
		Tune = 2,
		[Description("Intro/Outro/Pont")]
		Bridge = 3
	}
}
