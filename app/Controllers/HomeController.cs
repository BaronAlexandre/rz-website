using app.Helpers;
using app.Models;
using app.Services;
using Genius;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace app.Controllers
{
	public class HomeController : Controller
	{
		public readonly ILogger<HomeController> _logger;
		public readonly GeniusClient _genius;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
			_genius = new GeniusClient("lK00bVSgu4CJpmnp8Db8Lo2K0mYrrjBM5Z66sc6PWbZUP1nkkC08YABXd_JFyh0c");
		}

		[Route("/", Name = "Home")]
		public IActionResult Index(string nameArtist)
		{
			var rappeurs = DataService.Artists;
			var pager = new Pagination(rappeurs.Count, 1, 10, 11);
			if (!string.IsNullOrEmpty(nameArtist))
				rappeurs = DataService.Artists.Where(r => r.Name.ToLowerInvariant().Contains(nameArtist.ToLowerInvariant())).OrderByDescending(r => r.Popularity).ToList();
			else
				rappeurs = rappeurs.OrderByDescending(r => r.Popularity).Take(pager.PageSize).ToList();

			var model = new HomeViewModel()
			{
				Artists = rappeurs,
				Pagination = pager,
				ArtistsCount = DataService.Artists.Count
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
			var albumSimple = album.Artist.Albums.Items.FirstOrDefault(a => a.Id == idAlbum);
			album.Name = albumSimple.Name;

			foreach (var t in album.Tracks)
			{
				var colors = new List<string> { "#0FF395", "#1F96FD", "#FB5794", "#F7D86A", "#78FCFF" };
				Random r = new Random();
				var numbers = new List<SongTimeline>();
				var remaining = 100;
				var nb = r.Next(2, 5);
				numbers.Add(new SongTimeline()
				{
					Percentage = 0,
					Color = colors[r.Next(colors.Count)]
				});
				numbers.Add(new SongTimeline()
				{
					Percentage = 100,
					Color = colors[r.Next(colors.Count)]
				});
				for (int i = 0; i < nb; i++)
					if (i < (nb - 1))
					{
						int number = r.Next(0, remaining);
						numbers.Add(new SongTimeline()
						{
							Percentage = r.Next(1, 99),
							Color = colors[r.Next(colors.Count)]
						});
						remaining -= number;
					}
					else
						numbers.Add(new SongTimeline()
						{
							Percentage = remaining,
							Color = colors[r.Next(colors.Count)]
						});
				numbers = numbers.OrderBy(n => n.Percentage).ToList();
				string style = "";

				for (int i = 0; i < numbers.Count; i++)
				{
					var color = numbers[i].Color;
					var percentage = numbers[i].Percentage;
					if (i == 0)
					{
						style += $"{color} 0%, {color} {percentage}%";
					}
					else
					{
						var prevColor = numbers[i - 1].Color;
						var prevPercentage = numbers[i - 1].Percentage;
						style += $", {prevColor} {percentage}%, {color} {percentage}%";
					}
				}

				style += $", {numbers[0].Color} 100%";
				t.Timeline = style;
			}

			return View(album);
		}

		[Route("artist_{id}", Name = "Artist")]
		public IActionResult Artist(string id)
		{
			var artist = DataService.Artists.FirstOrDefault(a => a.Id == id);
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
			var albumSimple = artist.Albums.Items.FirstOrDefault(a => a.Id == idAlbum);
			var album = DataService.Albums.FirstOrDefault(a => a.Id == idAlbum);
			if (album == null)
				return NotFound();
			var track = album.Tracks.FirstOrDefault(t => t.Id == idTrack);
			if (track == null)
				return NotFound();

			track.Album = album;
			track.Album.Name = albumSimple.Name;
			track.Album.Images = albumSimple.Images;
			track.Album.Artist = artist;

			System.Net.WebRequest request = System.Net.WebRequest.Create(track.Album.Images.Last().Url);
			System.Net.WebResponse response = request.GetResponse();
			System.IO.Stream responseStream =
				response.GetResponseStream();
			Bitmap bitmap = new Bitmap(responseStream);

			var mostUsedColor = Helper.GetMostUsedColor(bitmap);

			var songsfind = _genius.SearchClient.Search(track.Name + " by " + artist.Name).Result;
			var songfindUrl = songsfind.Response.Hits.FirstOrDefault();
			if (songfindUrl != null)
			{
				HtmlWeb web = new HtmlWeb();
				HtmlDocument doc = web.Load(songfindUrl.Result.Url);
				var headerNames = doc.DocumentNode.SelectNodes("//*[@id='lyrics-root']/div[2]");
				var text = System.Web.HttpUtility.HtmlDecode(headerNames.FirstOrDefault().InnerText);
				if (text == null)
					return NotFound();

				track.Text = Helper.GetValuesToDictionary(Regex.Replace(text, @"\[Paroles de.*?\] ?", ""));
			};

			return View(track);
		}

		[Route("/index-lettres", Name = "IndexLetters")]
		public IActionResult IndexLetters()
		{
			var rappeurs = DataService.Artists.OrderByDescending(a => a.Popularity).Take(100).ToList();

			var model = new IndexLetterViewModel()
			{
				Artists = rappeurs,
				PaginationLetters = DataService.GetPaginationLetters()
			};

			return View(model);
		}

		[Route("/lettre_{letter}", Name = "IndexLetter")]
		public IActionResult IndexLetter(string letter)
		{
			if (!Regex.IsMatch(letter, @"^([a-z]|0-9)$"))
				return NotFound();

			var rappeurs = new List<SuperArtists>();

			if (letter != "0-9")
				rappeurs = DataService.Artists.Where(a => Helper.RemoveDiacritics(a.Name).StartsWith(letter)).OrderByDescending(a => a.Popularity).Take(100).ToList();
			else
				rappeurs = DataService.Artists.Where(a => Regex.IsMatch(Helper.RemoveDiacritics(a.Name), @"^(?![a-zA-Z])\w+$")).OrderByDescending(a => a.Popularity).Take(100).ToList();

			var model = new IndexLetterViewModel()
			{
				Artists = rappeurs,
				PaginationLetters = DataService.GetPaginationLetters()
			};

			model.PaginationLetters.FirstOrDefault(l => l.Slug == letter).IsActive = true;

			return View(model);
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}