using app.Models;
using Newtonsoft.Json;

namespace app.Services
{
    public class DataService : IDataService
    {
        public static List<SuperArtists> Artists { get; set; }
        public static List<string> Labels { get; set; }


        public static void Init()
        {
            Artists = ReadFromJsonFile<List<SuperArtists>>("rappeursgm2.json");
            Artists.AddRange(ReadFromJsonFile<List<SuperArtists>>("rappeursgm3.json"));
            Labels = ReadFromJsonFile<List<string>>("labels.json");
        }

        public static List<PaginationLetter> GetPaginationLetters()
        {
            return new List<PaginationLetter>()
            {
                new PaginationLetter{ IsActive = false, Label = "A", Slug = "a" },
                new PaginationLetter{ IsActive = false, Label = "B", Slug = "b" },
                new PaginationLetter{ IsActive = false, Label = "C", Slug = "c" },
                new PaginationLetter{ IsActive = false, Label = "D", Slug = "d" },
                new PaginationLetter{ IsActive = false, Label = "E", Slug = "e" },
                new PaginationLetter{ IsActive = false, Label = "F", Slug = "f" },
                new PaginationLetter{ IsActive = false, Label = "G", Slug = "g" },
                new PaginationLetter{ IsActive = false, Label = "H", Slug = "h" },
                new PaginationLetter{ IsActive = false, Label = "I", Slug = "i" },
                new PaginationLetter{ IsActive = false, Label = "J", Slug = "j" },
                new PaginationLetter{ IsActive = false, Label = "K", Slug = "k" },
                new PaginationLetter{ IsActive = false, Label = "L", Slug = "l" },
                new PaginationLetter{ IsActive = false, Label = "M", Slug = "m" },
                new PaginationLetter{ IsActive = false, Label = "N", Slug = "n" },
                new PaginationLetter{ IsActive = false, Label = "O", Slug = "o" },
                new PaginationLetter{ IsActive = false, Label = "P", Slug = "p" },
                new PaginationLetter{ IsActive = false, Label = "Q", Slug = "q" },
                new PaginationLetter{ IsActive = false, Label = "R", Slug = "r" },
                new PaginationLetter{ IsActive = false, Label = "S", Slug = "s" },
                new PaginationLetter{ IsActive = false, Label = "T", Slug = "t" },
                new PaginationLetter{ IsActive = false, Label = "U", Slug = "u" },
                new PaginationLetter{ IsActive = false, Label = "V", Slug = "v" },
                new PaginationLetter{ IsActive = false, Label = "W", Slug = "w" },
                new PaginationLetter{ IsActive = false, Label = "X", Slug = "x" },
                new PaginationLetter{ IsActive = false, Label = "Y", Slug = "y" },
                new PaginationLetter{ IsActive = false, Label = "Z", Slug = "z" },
                new PaginationLetter{ IsActive = false, Label = "0-9", Slug = "0-9" },
            };
        }

        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            Console.WriteLine(filePath);
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
