using app.Models;
using app.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Genius;
using HtmlAgilityPack;
using System.Net;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        public readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string nameArtist)
        {
            var rappeurs = DataService.Artists;
            if (!string.IsNullOrEmpty(nameArtist))
            {
                rappeurs = DataService.Artists.Where(r => r.Name.ToLowerInvariant().Contains(nameArtist.ToLowerInvariant())).ToList();
            }

            var pager = new Pagination(rappeurs.Count, 1, 20, 11);
            rappeurs = rappeurs.OrderByDescending(r => r.Popularity).Take(pager.PageSize).ToList();

            var model = new HomeViewModel()
            {
                Artists = rappeurs,
                Pagination = pager
            };

            return View(model);
        }

        [Route("{number}")]
        public IActionResult Pagination(int number)
        {
            var currentPage = number;
            var pager = new Pagination(DataService.Artists.Count, currentPage, 20, 11);
            var rappeurs = DataService.Artists.OrderByDescending(r => r.Popularity).Skip((currentPage-1)*pager.PageSize).Take(pager.PageSize).ToList();

            var model = new HomeViewModel()
            {
                Artists = rappeurs,
                Pagination = pager
            };

            return View("Index", model);
        }

        [Route("/{idArtist}/album/{idAlbum}")]
        public IActionResult Album(string idArtist, string idAlbum)
        {
            var album = DataService.Albums.FirstOrDefault(a => a.Id == idAlbum);
            album.Artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);

            return View(album);
        }

        [Route("/labels")]
        public IActionResult Labels()
        {
            return View(DataService.Labels);
        }


        [Route("{idArtist}/{idAlbum}/track/{idTrack}")]
        public IActionResult Track(string idArtist, string idAlbum, string idTrack)
        {
            var artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);
            var album = DataService.Albums.FirstOrDefault(a => a.Id == idAlbum);
            var track = album.Tracks.FirstOrDefault(t => t.Id == idTrack);

            var genius = new GeniusClient("lK00bVSgu4CJpmnp8Db8Lo2K0mYrrjBM5Z66sc6PWbZUP1nkkC08YABXd_JFyh0c");
            var songsfind = genius.SearchClient.Search(track.Name + " by " + artist.Name).Result;
            var songfindUrl = songsfind.Response.Hits.FirstOrDefault();
            if (songfindUrl != null)
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(songfindUrl.Result.Url);
                var headerNames = doc.DocumentNode.SelectNodes("//*[@id='lyrics-root']/div[2]");
                track.Text = System.Web.HttpUtility.HtmlDecode(headerNames.FirstOrDefault().InnerText);
                if (track.Text == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return View();
                }
            };

            return View(track);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}