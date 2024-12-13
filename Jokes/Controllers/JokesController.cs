using Microsoft.AspNetCore.Mvc;
using Joke;
using System;
using System.Linq;
using Joke.Data;
using Jokes.Models;

namespace Joke.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JokesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Random _random;
        public JokesController(AppDbContext context, Random random = null)
        {
            _context = context;
            _random = random ?? new Random();
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Return all the jokes in Jokes table 
            var jokes = _context.Jokes.ToList();
            return View(jokes);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var joke = _context.Jokes.FirstOrDefault(j => j.Id == id);
            return joke == null ? NotFound($"No joke found with ID: {id}") : new OkObjectResult(joke);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Joke joke)
        {
            if (joke == null)
            {
                return BadRequest("Joke is required.");
            }

            if (_context.Jokes.Any(j => j.Id == joke.Id))
            {
                return Conflict("Joke is already stored");
            }

            //deifne joke object 
            joke.Id = Guid.NewGuid();
            joke.CreatedDate = DateTime.Now;

            // Add joke object 
            _context.Jokes.Add(joke);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = joke.Id }, joke);
        }

        [HttpDelete("remove/{id:guid}")]
        public IActionResult DeleteJoke(Guid id)
        {
            // Try to find the joke
            var joke = _context.Jokes.FirstOrDefault(j => j.Id == id);

            // Check if joke is null
            if (joke == null)
            {
                return NotFound($"No joke found with ID: {id}");
            }

            // Remove the joke (now safely, because we know it's not null)
            _context.Jokes.Remove(joke);
            _context.SaveChanges();

            return new OkObjectResult($"Joke with ID {id} has been removed successfully.");
        }

        [HttpGet("random")]
        public IActionResult RandomJoke()
        {
            List<Joke> jokes = _context.Jokes.ToList();

            if (!jokes.Any())
            {
                return NotFound("No jokes available.");
            }

            // Generate a random index between 0 and jokeCount - 1
            var randomIndex = _random.Next(0, _context.Jokes.Count());

            if (randomIndex > jokes.Count())
            {
                return NotFound("Joke not found.");
            }

            // Get the random joke
            var randomJoke = jokes[randomIndex];

            // Return the random joke
            return new OkObjectResult(randomJoke);
        }
    }
}
