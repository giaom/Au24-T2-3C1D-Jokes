// JokesLibrary.cs
using System;
using System.Collections.Generic;
using System.Linq;


namespace JokesStorage
{
    public sealed class JokesLibrary
    {
        public List<Joke> jokes;

        // Singleton: No public constructor
        private JokesLibrary()
        {
            jokes = new List<Joke>
            {
                    new Joke{
                        Id = Guid.NewGuid(),
                        Text = "dotnet run addjoke \"Joke text here\" \"Author name here\"", // dotnet run addjoke "Joke text here" "Author name here"
                        Author = "(^ Instructions on adding Joke through terminal)",
                        CreatedDate = DateTime.Now
                    },
                    new Joke {
                        Id = Guid.NewGuid(),
                        Text = "Hardcoded Joke",
                        Author = "In JokesLibrary.cs",
                        CreatedDate = DateTime.Now
                    },
                    new Joke {
                        Id = Guid.NewGuid(),
                        Text = null,
                        Author = null,
                        CreatedDate = DateTime.Now
                    }
            };
        }

        public static JokesLibrary Instance { get { return NestedJokesLibrary.instance; } }

        // Nested class for lazy load singleton pattern
        private class NestedJokesLibrary
        {
            static NestedJokesLibrary() { }
            internal static readonly JokesLibrary instance = new JokesLibrary();
        }

        /// <summary>
        /// retrieves a joke stored with a Id value equal to the input Guid id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Joke GetByID(Guid id)
        {
            return jokes.FirstOrDefault(joke => joke.Id == id);
        }

        /// <summary>
        /// retrieves a joke stored at the input int index in the List<Joke>
        /// if the index is larger than the number of stored jokes, index will be assigned
        /// index % jokes.Count, ensuring only indicies within range are accessed
        /// if there are no jokes stored, returns null
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Joke GetByIndex(int index)
        {
            if (jokes.Count == 0)
                return null;

            index = index % jokes.Count;
            Joke joke = jokes[index];
            return joke;
        }

        /// <summary>
        /// retrieves a list of jokes stored with an Author value equal to the input string author
        /// case insensitively
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        public List<Joke> GetByAuthor(string author)
        {
            List<Joke> jokesOfAuthor = jokes.FindAll(joke => joke.Author.ToLower() == author.ToLower());
            return jokesOfAuthor;
        }

        public List<Joke> GetAll()
        {
            return jokes;
        }

        public void AddJoke(Joke joke)
        {
            jokes.Add(joke);
        }
    }
}
