using app.Models;
using Newtonsoft.Json;

namespace app.Services
{
    public interface IDataService
    {
        static List<SuperArtists> Artists { get; }
        static List<Album> Albums { get; }
        static List<string> Labels { get; }
    }
}
