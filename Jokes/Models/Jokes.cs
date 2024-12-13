// Jokes.cs
using System;
using System.Text.Json.Serialization;

namespace Joke
{
    public class Joke
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

