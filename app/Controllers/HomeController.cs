using app.Models;
using app.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        [Route("/album/{id}")]
        public IActionResult Album(string id)
        {
            var album = DataService.Albums.FirstOrDefault(a => a.Id == id);

            return View(album.Tracks);
        }

        [Route("/labels")]
        public IActionResult Labels()
        {
            return View(DataService.Labels);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}