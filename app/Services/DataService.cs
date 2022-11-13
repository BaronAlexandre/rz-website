using app.Models;
using Newtonsoft.Json;

namespace app.Services
{
    public class DataService : IDataService
    {
        public static List<SuperArtists> Artists { get; set; }
        public static List<Album> Albums { get; set; }
        public static List<string> Labels { get; set; }

        public static void Init()
        {
            Artists = ReadFromJsonFile<List<SuperArtists>>("rappeursfr.json");
            Albums = ReadFromJsonFile<List<Album>>("tracks.json");
            Labels = ReadFromJsonFile<List<string>>("labels.json");
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
