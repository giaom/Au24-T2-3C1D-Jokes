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
        
        /// Description: Get all jokes
        /// Get: /api/jokes
        /// Returns: List<Joke>
        [HttpGet]
        public IActionResult Index()
        {
            var jokes = _context.Jokes.ToList();
            return View(jokes);
        }

        /// Description: Get a joke by ID
        /// Get: /api/jokes/{id:guid}
        /// Parameter: id type Guid
        /// Returns: Joke
        [HttpGet("/{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var joke = _context.Jokes.FirstOrDefault(j => j.Id == id);
            return joke == null ? NotFound($"No joke found with ID: {id}") : new OkObjectResult(joke);
        }

        /// Description: Create a new joke
        /// Post: /api/jokes
        /// Parameter: joke type Joke
        /// Returns: Joke
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

            // define joke object 
            joke.Id = Guid.NewGuid();
            joke.CreatedDate = DateTime.Now;

            _context.Jokes.Add(joke);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = joke.Id }, joke);
        }
  
        /// Description: Delete a joke  
        /// Delete: /api/jokes/remove/{id:guid}
        /// Parameter: id type Guid
        /// Returns: string
        [HttpDelete("remove/{id:guid}")]
        public IActionResult DeleteJoke(Guid id)
        {
            var joke = _context.Jokes.FirstOrDefault(j => j.Id == id);

            if (joke == null)
            {
                return NotFound($"No joke found with ID: {id}");
            }

            _context.Jokes.Remove(joke);
            _context.SaveChanges();

            return new OkObjectResult($"Joke with ID {id} has been removed successfully.");
        }

        /// Description: Get a random joke
        /// Get: /api/jokes/random    
        /// Returns: Joke
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
                return NotFound("Out of range index.");
            }

            // Get the random joke
            var randomJoke = jokes[randomIndex];

            // Return the random joke
            return new OkObjectResult(randomJoke);
        }

        /// Description: Get jokes by author
        /// Get: /api/jokes/byauthor/{author}
        /// Parameter: author type string
        /// Returns: List<Joke>
        [HttpGet("byauthor/{author:}")]
        public IActionResult GetJokesByAuthor(string author)
        {
            var jokes = _context.Jokes.Where(j => j.Author == author).ToList();

            if (jokes == null)
            {
                return NotFound($"No joke found with author: {author}");
            }

            return Ok(jokes);
        }
    }
}
