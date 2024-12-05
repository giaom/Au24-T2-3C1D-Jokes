// Jokes.cs
using System;
using System.Text.Json.Serialization;


namespace Quotes
{
    public class Joke
{
    public Guid Id { get; set; }
    
    public required string Text { get; set; }  // Ensures that Text is provided
    public required string Author { get; set; }  // Ensures that Author is provided
    
    public DateTime CreatedDate { get; set; }
}

}

