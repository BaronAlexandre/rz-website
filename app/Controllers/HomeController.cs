using app.Helpers;
using app.Models;
using app.Services;
using Genius;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
			var pager = new Pagination(rappeurs.Count, 1, 20, 11);
			if (!string.IsNullOrEmpty(nameArtist))
				rappeurs = DataService.Artists.Where(r => r.Name.ToLowerInvariant().Contains(nameArtist.ToLowerInvariant())).OrderByDescending(r => r.Popularity).ToList();
			else
				rappeurs = rappeurs.OrderByDescending(r => r.Popularity).Take(pager.PageSize).ToList();

			var model = new HomeViewModel()
			{
				Artists = rappeurs,
				Pagination = pager
			};

			return View(model);
		}

		[Route("{number:int}", Name = "Pagination")]
		public IActionResult Pagination(int number)
		{
			var currentPage = number;
			var pager = new Pagination(DataService.Artists.Count, currentPage, 20, 11);
			var rappeurs = DataService.Artists.OrderByDescending(r => r.Popularity).Skip((currentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList();

			var model = new HomeViewModel()
			{
				Artists = rappeurs,
				Pagination = pager
			};

			return View("Index", model);
		}

		[Route("artist_{idArtist}/album_{idAlbum}", Name = "Album")]
		public IActionResult Album(string idArtist, string idAlbum)
		{
			var album = DataService.Albums.FirstOrDefault(a => a.Id == idAlbum);
			if (album == null)
				return NotFound();
			album.Artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);

			return View(album);
		}

		[Route("artist_{idArtist}", Name = "Artist")]
		public IActionResult Artist(string idArtist)
		{
			var artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);
			if (artist == null)
				return NotFound();

			return View(artist);
		}

		[Route("/labels", Name = "Labels")]
		public IActionResult Labels()
		{
			return View(DataService.Labels);
		}


		[Route("artist_{idArtist}/album_{idAlbum}/track_{idTrack}", Name = "Track")]
		public IActionResult Track(string idArtist, string idAlbum, string idTrack)
		{
			var artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);
			if (artist == null)
				return NotFound();
			var album = DataService.Albums.FirstOrDefault(a => a.Id == idAlbum);
			if (album == null)
				return NotFound();
			var track = album.Tracks.FirstOrDefault(t => t.Id == idTrack);
			if (track == null)
				return NotFound();

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
					return NotFound();
			};

			return View(track);
		}

		[Route("/index-lettres", Name = "IndexLetters")]
		public IActionResult IndexLetters()
		{
			var rappeurs = DataService.Artists.OrderByDescending(a => a.Popularity).Take(100).ToList();

			return View(rappeurs);
		}

		[Route("/lettre_{letter}", Name = "IndexLetter")]
		public IActionResult IndexLetter(string letter)
		{
			if (!Regex.IsMatch(letter, @"[a-z]|0-9"))
				return NotFound();

			var rappeurs = new List<SuperArtists>();

			if (letter != "0-9")
				rappeurs = DataService.Artists.Where(a => Helper.RemoveDiacritics(a.Name).StartsWith(letter)).OrderByDescending(a => a.Popularity).Take(100).ToList();
			else
				rappeurs = DataService.Artists.Where(a => Regex.IsMatch(Helper.RemoveDiacritics(a.Name), @"^(?![a-zA-Z])\w+$")).OrderByDescending(a => a.Popularity).Take(100).ToList();

			return View(rappeurs);
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}