// JokerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;


namespace JokesStorage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JokesController : ControllerBase
    {
        private readonly ILogger<JokesController> _logger;
        private readonly JokesLibrary library;


        public JokesController(ILogger<JokesController> logger)
        {
            _logger = logger;
            library = JokesLibrary.Instance;
        }


        [HttpPost]
        public IActionResult Post([FromBody] Joke joke)
        {
            joke.Id = Guid.NewGuid();
            joke.CreatedDate = DateTime.Now;
            library.AddJoke(joke);
            return CreatedAtAction(nameof(Get), new { id = joke.Id }, joke);
        }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok(library.GetAll());
        }


        [HttpGet("id/{id:guid}")]
        public IActionResult Get(Guid id)
        {
            Joke result = library.GetByID(id);
            return (result == null) ? NotFound("No jokes with ID: " + "\"" + id + "\"") : Ok(result);
        }

        [HttpGet("author/{author}")]
        public IActionResult Get(string author)
        {
            // Query the database for jokes by the given author, case insensitively
            List<Joke> result = library.GetByAuthor(author);

            // If no jokes are found, return 404, else return list of jokes          
            return (!result.Any()) ? NotFound("No jokes with Author: " + "\"" + author + "\"") : Ok(result);
        }
    }
}
