using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace app.Helpers
{
	public static class Helper
	{
		public static string RemoveDiacritics(string text)
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

			for (int i = 0; i < normalizedString.Length; i++)
			{
				char c = normalizedString[i];
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder
				.ToString().ToLowerInvariant()
				.Normalize(NormalizationForm.FormC);
		}

		public static Color GetMostUsedColor(Bitmap bitMap)
		{
			var colorIncidence = new Dictionary<int, int>();
			for (var x = 0; x < bitMap.Size.Width; x++)
				for (var y = 0; y < bitMap.Size.Height; y++)
				{
					var pixelColor = bitMap.GetPixel(x, y).ToArgb();
					if (colorIncidence.Keys.Contains(pixelColor))
						colorIncidence[pixelColor]++;
					else
						colorIncidence.Add(pixelColor, 1);
				}
			return Color.FromArgb(colorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).First().Key);
		}
		public static List<Tuple<string, string>> GetValuesToDictionary(string text)
		{
			var pattern = @"\[([^\]]+)\]([^\[]+)";

			var regex = new Regex(pattern);

			var pairs = new List<Tuple<string, string>>();

			var matches = regex.Matches(text);
			foreach (Match match in matches)
			{
				var key = match.Groups[1].Value;
				var value = match.Groups[2].Value;
				if (!pairs.Any(p => p.Item1 == key))
				{
					pairs.Add(Tuple.Create(key, value));
				}
			}

			return pairs;
		}
	}
}
