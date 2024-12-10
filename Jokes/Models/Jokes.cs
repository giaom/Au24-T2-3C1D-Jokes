// Jokes.cs
using System;
using System.Text.Json.Serialization;


namespace JokesStorage
{
    public class Joke
    {
        public Guid Id { get; set; }
        public string? Text { get; set; } // Nullable
        public string? Author { get; set; } // Nullable
        public DateTime CreatedDate { get; set; }
    }
}
