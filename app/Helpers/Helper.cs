using System.ComponentModel;
using System.Globalization;
using System.Reflection;
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
        public static string GetEnumDescription(this Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
