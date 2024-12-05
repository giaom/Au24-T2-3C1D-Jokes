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
                new Joke
                {
                    Id = Guid.NewGuid(),
                    Text = "Why don't scientists trust atoms? Because they make up everything!",
                    Author = "Unknown",
                    CreatedDate = DateTime.Now
                }
                // Add more jokes here if needed
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
