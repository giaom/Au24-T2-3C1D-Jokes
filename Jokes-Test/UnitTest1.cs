using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Joke;
using Joke.Data;
using Joke.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;
using Moq;

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
        /// creates and returns a new Joke.Joke based on the input id, text, and author
        /// sets the CreatedDate as 2020, January 1st, 00:00:00
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <param name="author"></param>
        /// <returns> Joke.Joke </returns>
        private Joke.Joke CreateJoke(Guid id, string text, string author)
        {
            return new Joke.Joke
            {
                Id = id,
                Text = text,
                Author = author,
                CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0)
            };
        }

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
                        CreateJoke(testGUIDs[i], testJokes[i], testAuthors[i])
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
        private JokesController SetUpController(Random random = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context = FillDatabase(_context);

            Random _random = random ?? new Random();

            return new JokesController(_context, _random);
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
        // Index Happy case - index returns a view of the jokes
        public void Jokes_Index_ReturnsView()
        {
            // SET UP   
            JokesController controller = SetUpController();

            // ACT
            IActionResult result = controller.Index();

            // ASSERT
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        // Index Edge case - index returns an empty view when database is empty
        public void Jokes_Index_ReturnsView_WhenEmpty()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);   // empty DB
            _context.Database.EnsureDeleted();

            JokesController controller = new JokesController(_context);

            // ACT
            IActionResult result = controller.Index();

            // ASSERT
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        // GetById Happy case - get by the id stored in the database and return the joke 
        public void Jokes_GetById_ReturnJoke()
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
        // GetById Edge case - get by the id of the only stored joke and return that joke
        public void Jokes_GetById_ReturnJoke_DBStoresOneJoke()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            int jokeIndex = 0;

            Joke.Joke expectedJoke =
                        CreateJoke(testGUIDs[jokeIndex], testJokes[jokeIndex], testAuthors[jokeIndex]);

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context.Add(expectedJoke);
            _context.SaveChanges();

            JokesController controller = new JokesController(_context);

            // ACT
            OkObjectResult queryResult = (OkObjectResult)controller.Get(testGUIDs[jokeIndex]);
            Joke.Joke actualJoke = queryResult.Value as Joke.Joke;

            // ASSERT
            Assert.Equal(expectedJoke, actualJoke);
        }

        [Fact]
        // GetById Edge case - getById in an empty database and return notfound
        public void Jokes_GetById_ReturnNotFound_EmptyDB()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);   // empty DB
            _context.Database.EnsureDeleted();

            JokesController controller = new JokesController(_context);

            Guid nonExistentGuid = new Guid("a7b2570d-f2ca-4c08-8209-f1ce161e96e3");

            // ACT
            IActionResult queryResult = controller.Get(nonExistentGuid);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(queryResult);
        }

        [Fact]
        // GetById Negative case - get by an id not associated with a stored joke and return notfound
        public void Jokes_GetById_ReturnNotFound_IncorrectGuid()
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
        // Post Happy case - checks that the correct status code is returned after a successful post
        public void Jokes_Post_StatusCode201()
        {
            // SET UP
            JokesController controller = SetUpController();

            Joke.Joke toPost = CreateJoke(new Guid("d7d67bf4-ec2f-4c51-8e76-984810206c1b"),
             "Added Joke", "Added Author");

            // ACT
            IActionResult queryResult = controller.Post(toPost);

            // ASSERT
            Assert.IsType<CreatedAtActionResult>(queryResult);
        }

        [Fact]
        // Post Happy case - an empty library contains a joke after post
        public void Jokes_Post_DBStoresNewJoke_EmptyDB()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();

            int jokeIndex = 0;
            Joke.Joke toPost = CreateJoke(testGUIDs[jokeIndex], testJokes[jokeIndex], testAuthors[jokeIndex]);

            JokesController controller = new JokesController(_context);

            // ACT
            IActionResult queryResult = controller.Post(toPost);

            // ASSERT
            var storedJoke = _context.Jokes.FirstOrDefault(j => j.Id == toPost.Id);
            Assert.True(storedJoke != null);
        }

        [Fact]
        // Post Happy case - checks that a joke can be added using Post and return Joke.Joke
        // when posting to an empty database
        public void Jokes_Post_ReturnJoke_EmptyDB()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();

            int jokeIndex = 0;
            Joke.Joke expectedJoke = CreateJoke(testGUIDs[jokeIndex], testJokes[jokeIndex], testAuthors[jokeIndex]);

            JokesController controller = new JokesController(_context);

            // ACT
            CreatedAtActionResult queryResult = (CreatedAtActionResult)controller.Post(expectedJoke);
            Joke.Joke actualJoke = queryResult.Value as Joke.Joke;

            // ASSERT
            Assert.True(expectedJoke == actualJoke);
        }


        [Fact]
        // Post Edge case - checks that a duplicate joke can not be stored
        public void Jokes_Post_PostJoke_DBContainsDuplicate()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();

            int jokeIndex = 0;
            Joke.Joke expectedJoke = CreateJoke(testGUIDs[jokeIndex], testJokes[jokeIndex], testAuthors[jokeIndex]);

            _context.Add(expectedJoke);
            _context.SaveChanges();

            JokesController controller = new JokesController(_context);

            // ACT
            IActionResult queryResult = controller.Post(expectedJoke);

            // ASSERT
            Assert.IsType<ConflictObjectResult>(queryResult);
        }

        [Fact]
        // Post Negative case - post a joke that is null
        public void Jokes_Post_ReturnBadRequest_JokeIsNull()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();

            Joke.Joke toPost = new Joke.Joke();
            toPost = null;

            JokesController controller = new JokesController(_context);

            // ACT
            IActionResult queryResult = controller.Post(toPost);

            // ASSERT
            Assert.IsType<BadRequestObjectResult>(queryResult);
        }

        [Fact]
        // Remove Happy case - remove a joke, the joke will not be found in the database
        public void Jokes_Remove_RemovesJoke()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context = FillDatabase(_context);  // fills database with jokes

            JokesController controller = new JokesController(_context);

            int jokeIndex = 0;
            Guid jokeId = testGUIDs[jokeIndex]; // Guid of a joke in the database

            // ACT
            controller.DeleteJoke(jokeId);

            // ASSERT
            Assert.True(_context.Jokes.FirstOrDefault(j => j.Id == jokeId) == null);
        }

        [Fact]
        // Remove Edge case - remove a joke twice and return notfound
        public void Jokes_Remove_ReturnNotFound_RemoveTwice()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context = FillDatabase(_context);  // fills database with jokes

            JokesController controller = new JokesController(_context);

            int jokeIndex = 0;
            Guid jokeId = testGUIDs[jokeIndex]; // Guid of a joke in the database

            // ACT
            controller.DeleteJoke(jokeId);
            IActionResult queryResult = controller.DeleteJoke(jokeId);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(queryResult);
        }

        [Fact]
        // Remove Negative case - remove id of joke that isn't stored and return notfound 
        public void Jokes_Remove_ReturnNotFound_IncorrectGuid()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context = FillDatabase(_context);  // fills database with jokes

            JokesController controller = new JokesController(_context);

            Guid nonExistentGuid = new Guid("8b56932f-0b35-49b3-a558-9844f7d3b4c8");

            // ACT
            IActionResult queryResult = controller.DeleteJoke(nonExistentGuid);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(queryResult);
        }

        [Fact]
        // Random Happy case - test with the same seed and always return the same joke
        public void Jokes_Random_ReturnSameJoke_SameSeed()
        {
            // SET UP
            int seed = 1;
            Random random = new Random(seed);
            JokesController controller = SetUpController(random);

            // ACT
            OkObjectResult queryResult1 = (OkObjectResult)controller.RandomJoke();
            OkObjectResult queryResult2 = (OkObjectResult)controller.RandomJoke();

            Joke.Joke joke1 = queryResult1.Value as Joke.Joke;
            Joke.Joke joke2 = queryResult2.Value as Joke.Joke;

            // ASSERT
            Assert.Equal(joke1, joke2);
        }

        [Fact]
        // Random Happy case - database stores one joke and return that joke
        public void Jokes_Random_ReturnSameJoke_DBStoresOneJoke()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            int jokeIndex = 0;

            Joke.Joke expectedJoke =
                        CreateJoke(testGUIDs[jokeIndex], testJokes[jokeIndex], testAuthors[jokeIndex]);

            AppDbContext _context = new(options);
            _context.Database.EnsureDeleted();
            _context.Add(expectedJoke);
            _context.SaveChanges();

            JokesController controller = new JokesController(_context);

            // ACT
            OkObjectResult queryResult1 = (OkObjectResult)controller.RandomJoke();
            OkObjectResult queryResult2 = (OkObjectResult)controller.RandomJoke();

            Joke.Joke joke1 = queryResult1.Value as Joke.Joke;
            Joke.Joke joke2 = queryResult2.Value as Joke.Joke;

            // ASSERT
            Assert.Equal(joke1, joke2);
        }

        [Fact]
        // Random Negative case - database doesn't store jokes
        public void Jokes_Random_ReturnNotFound_EmptyDB()
        {
            // SET UP
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestJokesDb").Options;

            AppDbContext _context = new(options);   // empty DB
            _context.Database.EnsureDeleted();

            JokesController controller = new JokesController(_context);

            // ACT
            IActionResult result = controller.RandomJoke();

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Random Negative case - out of bounds index to access Joke
        public void Jokes_Random_ReturnNotFound_OutOfBoundsIndex()
        {
            // SET UP
            var mockRandom = new Mock<Random>();
            mockRandom.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(NUMJOKES + 1);  // Set out-of-bounds index (greater than the list count)

            JokesController controller = SetUpController(mockRandom.Object);

            // ACT
            IActionResult result = controller.RandomJoke();

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(result);
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