using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Joke;
using Joke.Data;
using Joke.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;

namespace Jokes_Test
{
    public class Jokes_DB_And_Controller_Tests()
    {
        const int NUMJOKES = 4;

        private readonly List<Guid> testGUIDs = new()
        {
            new Guid("9811b9e7-1eb8-4174-a43b-13f5ee8364fd"),
            new Guid("6c2c5959-ffc8-4a26-8b86-5009f954eb7b"),
            new Guid("40328c24-5577-4475-8257-6203f2aa0187"),
            new Guid("ab8ea39a-9fa2-4494-8872-a286de53b840")
        };

        private readonly List<string> testJokes = new()
        {
            "Joke 1",
            "Joke 2",
            "Joke 3",
            "Joke 4"
        };

        private readonly List<string> testAuthors = new()
        {
            "Author 1",
            "Author 2",
            "Author 3",
            "Author 4"
        };

        /// <summary>
        /// takes a APPDbContext object and fills it with a range of pre-generated jokes
        /// </summary>
        /// <param name="_context"></param>
        /// <returns> AppDbContext </returns>
        private AppDbContext FillDatabase(AppDbContext _context)
        {
            if (!_context.Jokes.Any())
            {
                for (int i = 0; i < NUMJOKES; i++)
                {
                    _context.Add(
                        new Joke.Joke
                        {
                            Id = testGUIDs[i],
                            Text = testJokes[i],
                            Author = testAuthors[i],
                            CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0)
                        }
                    );
                }
                _context.SaveChanges();
            }
            return _context;
        }

        /// <summary>
        /// Sets up and returns a new jokesController with an AppDbContext
        /// </summary>
        /// <returns> JokesController </returns>
        private JokesController SetUpController()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context = FillDatabase(_context);

            return new JokesController(_context);
        }

        /// <summary>
        /// Sets up joke objects that are also stored in the inMemoryDatabase in a List<Joke.Joke>
        /// to be directly accessed as expected values for tests
        /// </summary>
        /// <returns> List<Joke.Joke> </returns>
        private List<Joke.Joke> SetUpJokes()
        {
            List<Joke.Joke> jokes = new List<Joke.Joke>();
            for (int i = 0; i < NUMJOKES; i++)
            {
                jokes.Add(new Joke.Joke
                {
                    Id = testGUIDs[i],
                    Text = testJokes[i],
                    Author = testAuthors[i],
                    CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0)
                });
            }
            return jokes;
        }

        [Fact]
        // GetById Happy case
        public void Jokes_GetById_ReturnsCorrectJoke()
        {
            // SET UP
            JokesController controller = SetUpController();

            List<Joke.Joke> expectedJokes = SetUpJokes();
            List<Joke.Joke> actualJokes = new List<Joke.Joke>();

            // ACT
            for (int i = 0; i < NUMJOKES; i++)
            {
                OkObjectResult queryResult = (OkObjectResult)controller.Get(testGUIDs[i]);
                actualJokes.Add(queryResult.Value as Joke.Joke);
            }

            // ASSERT
            Assert.True(expectedJokes.SequenceEqual(actualJokes, new JokeEqualityComparer()));
        }

        [Fact]
        // GetById Edge case
        public void Jokes_GetById_WhenEmpty()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);

            JokesController controller = new JokesController(_context);

            Guid nonExistentGuid = new Guid("a7b2570d-f2ca-4c08-8209-f1ce161e96e3");

            // ACT
            IActionResult queryResult = controller.Get(nonExistentGuid);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(queryResult);
        }

        [Fact]
        // GetById Negative case
        public void Jokes_GetById_DoesNotReturnNonExistentJoke()
        {
            // SET UP
            JokesController controller = SetUpController();

            Guid nonExistentGuid = new Guid("8b56932f-0b35-49b3-a558-9844f7d3b4c8");

            // ACT
            IActionResult queryResult = controller.Get(nonExistentGuid);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(queryResult);
        }

        [Fact]
        public void Jokes_Remove_RemovesJoke()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context = FillDatabase(_context);  // fills database with jokes

            JokesController controller = new JokesController(_context);

            int jokeIndex = 0;
            Guid jokeId = testGUIDs[jokeIndex]; // Guid of a joke in the database

            // ACT
            controller.DeleteJoke(jokeId);

            // ASSERT
            Assert.True(_context.Jokes.FirstOrDefault(j => j.Id == jokeId) == null);
        }
    }

    public class JokeEqualityComparer : IEqualityComparer<Joke.Joke>
    {
        /// <summary>
        /// takes two Joke.Joke objects and compares the Id, Text, Author, and CreatedDate values
        /// returns true if all are equal
        /// </summary>
        /// <param name="Joke.Joke x"></param>
        /// <param name="Joke.Joke y"></param>
        /// <returns>bool</returns>
        public bool Equals(Joke.Joke x, Joke.Joke y)
        {
            if (x == null || y == null) return false;
            return x.Id == y.Id &&
                   x.Text == y.Text &&
                   x.Author == y.Author &&
                   x.CreatedDate == y.CreatedDate;
        }

        /// <summary>
        /// combines an input Joke.Joke's attributes into one int value
        /// allows Joke.Joke objects with the same attribute values to have the 
        /// same hashed value and will be considered equal 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>int</returns>
        public int GetHashCode(Joke.Joke obj)
        {
            return HashCode.Combine(obj.Id, obj.Text, obj.Author, obj.CreatedDate);
        }
    }
}