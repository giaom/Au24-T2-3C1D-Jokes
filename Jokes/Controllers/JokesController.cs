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

        public JokesController(AppDbContext context)
        {
            _context = context;
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
            return joke == null ? NotFound() : new OkObjectResult(joke);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Joke joke)
        {
            if (joke == null)
            {
                return BadRequest("Joke is required.");
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
            // Get the total count of jokes in the database
            var jokeCount = _context.Jokes.Count();

            if (jokeCount == 0)
            {
                return NotFound("No jokes available.");
            }

            // Create a random object
            Random random = new Random();

            // Generate a random index between 0 and jokeCount - 1
            int randomIndex = random.Next(0, jokeCount);

            List<Joke> jokes = _context.Jokes.ToList();

            // Get the random joke
            var randomJoke = jokes[randomIndex];

            if (randomJoke == null)
            {
                return NotFound("Joke not found.");
            }

            // Return the random joke
            return new OkObjectResult(randomJoke);
        }
    }
}
