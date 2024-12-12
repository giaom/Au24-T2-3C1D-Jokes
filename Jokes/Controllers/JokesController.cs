using Microsoft.AspNetCore.Mvc;
using Quotes;
using System;
using System.Linq;
using Quotes.Data;
using Jokes.Models;

namespace Quotes.Controllers
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
            return joke == null ? NotFound() : Ok(joke);
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

            return Ok($"Joke with ID {id} has been removed successfully.");
        }

        /// Description: Get a random joke
        /// Get: /api/jokes/random    
        /// Returns: Joke
        [HttpGet("random")]
        public IActionResult RandomJoke()
        {
            var jokeCount = _context.Jokes.Count();

            if (jokeCount == 0)
            {
                return NotFound("No jokes available.");
            }

            Random random = new Random();
            int randomIndex = random.Next(0, jokeCount);
            
            List<Joke> jokes = _context.Jokes.ToList();
            var randomJoke = jokes[randomIndex];

            if (randomJoke == null)
            {
                return NotFound("Joke not found.");
            }

            return Ok(randomJoke);
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
