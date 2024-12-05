using Microsoft.AspNetCore.Mvc;
using Quotes;
using System;
using System.Linq;
using Quotes.Data;

namespace Quotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JokesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JokesController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var jokes = _context.Jokes.ToList();
            return Ok(jokes);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var joke = _context.Jokes.FirstOrDefault(j => j.Id == id);
            return joke == null ? NotFound() : Ok(joke);
        }

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

        // get quote based author endpoint 


        // Delete author based on id endpoint 

        // update quote text endpoint 
    }
}
