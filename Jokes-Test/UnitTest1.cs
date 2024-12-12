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
        public void Jokes_GetById_Returns_Correct_Id()
        {
            // SET UP
            JokesController controller = SetUpController();

            List<Joke.Joke> expectedJokes = SetUpJokes();
            List<Joke.Joke> actualJokes = new List<Joke.Joke>();

            // ACT
            for (int i = 0; i < NUMJOKES; i++)
            {
                OkObjectResult actual = (OkObjectResult)controller.Get(testGUIDs[i]);
                actualJokes.Add(actual.Value as Joke.Joke);
            }

            // ASSERT
            Assert.True(expectedJokes.SequenceEqual(actualJokes, new JokeEqualityComparer()));
        }
    }
    public class JokeEqualityComparer : IEqualityComparer<Joke.Joke>
    {
        public bool Equals(Joke.Joke x, Joke.Joke y)
        {
            if (x == null || y == null) return false;
            return x.Id == y.Id &&
                   x.Text == y.Text &&
                   x.Author == y.Author &&
                   x.CreatedDate == y.CreatedDate;
        }

        public int GetHashCode(Joke.Joke obj)
        {
            return HashCode.Combine(obj.Id, obj.Text, obj.Author, obj.CreatedDate);
        }
    }
}