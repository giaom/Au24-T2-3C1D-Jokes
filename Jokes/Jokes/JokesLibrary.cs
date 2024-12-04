// JokesLibrary.cs
using System;
using System.Collections.Generic;
using System.Linq;


namespace Quotes
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


        public Joke GetByID(Guid id)
        {
            return jokes.FirstOrDefault(joke => joke.Id == id);
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
