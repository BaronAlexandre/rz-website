using app.Helpers;
using app.Models;
using app.Services;
using Genius;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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
            var album = DataService.Artists.Where(a => a.Id == idArtist).SelectMany(a => a.Albums).FirstOrDefault(a => a.Id == idAlbum);
            if (album == null)
                return NotFound();
            album.Artist = DataService.Artists.FirstOrDefault(a => a.Id == idArtist);
            var albumSimple = album.Artist.Albums.FirstOrDefault(a => a.Id == idAlbum);
            album.Name = albumSimple.Name;
            album.Images = albumSimple.Images;
            album.ReleaseDate = albumSimple.ReleaseDate;

            Random r = new Random();
            string[] colorsBg = { "#000000", "#8B0000", "#000080", "#006400", "#FF8C00", "#9400D3", "#A9A9A9", "#FFD700", "#87CEEB", "#FFC0CB" };

            ViewBag.MostUsedColor = "background: linear-gradient(" + colorsBg[r.Next(colorsBg.Length)] + " 8%,#12141D 12%);";

            foreach (var t in album.Tracks)
            {
                var colors = new List<string> { "#0FF395", "#1F96FD", "#FB5794", "#F7D86A", "#78FCFF" };
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
            var album = artist.Albums.FirstOrDefault(a => a.Id == idAlbum);
            if (album == null)
                return NotFound();
            var track = album.Tracks.FirstOrDefault(t => t.Id == idTrack);
            if (track == null)
                return NotFound();

            track.Album = album;
            track.Album.Artist = artist;
            track.TextItem = new List<TrackElement>();

            var text = track.TextTool;
            if (text == null)
                return NotFound();

            var lyrics = Helper.GetValuesToDictionary(Regex.Replace(text, @"\[Paroles de.*?\] ?", ""));

            if (lyrics.Count == 0)
                return NotFound();

            foreach (var element in lyrics)
            {
                track.TextItem.Add(new TrackElement()
                {
                    SongTimeline = new SongTimeline()
                    {
                        Percentage = (int)Math.Ceiling((element.Item2.Length / (double)text.Length) * 100),
                        Type = Regex.IsMatch(element.Item1, ".*couplet.*", RegexOptions.IgnoreCase) ? TrackElementType.Verse : Regex.IsMatch(element.Item1, ".*refrain.*", RegexOptions.IgnoreCase) ? TrackElementType.Tune : Regex.IsMatch(element.Item1, ".*(pont|intro|outro).*", RegexOptions.IgnoreCase) ? TrackElementType.Bridge : TrackElementType.None
                    },
                    Lines = Regex.Split(element.Item2, "(?<=[a-z])(?=[A-Z])|(?<=\\))(?=[A-Za-z])").ToList(),
                    Title = element.Item1,
                });
            }

            var verification100 = track.TextItem.Sum(x => x.SongTimeline.Percentage);
            if (verification100 < 100)
            {
                var minimum = track.TextItem.Where(x => x.SongTimeline.Percentage == track.TextItem.Min(y => y.SongTimeline.Percentage)).FirstOrDefault();
                minimum.SongTimeline.Percentage = (minimum.SongTimeline.Percentage + (100 - verification100));
            }
            else if (verification100 > 100)
            {
                var maximum = track.TextItem.Where(x => x.SongTimeline.Percentage == track.TextItem.Min(y => y.SongTimeline.Percentage)).FirstOrDefault();
                maximum.SongTimeline.Percentage = (maximum.SongTimeline.Percentage - (verification100 - 100));
            }

            var temp = track.TextItem.GroupBy(x => x.SongTimeline.Type).Select(g => new { Key = g.Key, Value = g.Sum(x => x.SongTimeline.Percentage) }).OrderByDescending(x => x.Value).ToList();
            track.Repartition = new List<SongTimeline>();
            foreach (var item in temp)
            {
                track.Repartition.Add(new SongTimeline()
                {
                    Type = item.Key,
                    Percentage = item.Value
                });
            }


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