using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Jokes.Models;
using JokesStorage;

namespace Jokes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly JokesLibrary _library;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _library = JokesLibrary.Instance;
        }

        public IActionResult Index()
        {
            var jokes = _library.GetAll();
            Joke randomJoke = null;

            if (jokes.Count > 0)
            {
                Random rand = new Random();
                int randIndex = rand.Next(jokes.Count);
                randomJoke = jokes[randIndex];
            }

            return View(randomJoke);
        }

        [HttpGet]
        public IActionResult GetRandomJoke()
        {
            var jokes = _library.GetAll();
            if (jokes.Count == 0)
            {
                return Json(new { success = false, message = "No jokes available." });
            }

            Random rand = new Random();
            int randIndex = rand.Next(jokes.Count);
            var randomJoke = jokes[randIndex];

            return Json(new { success = true, joke = randomJoke });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}