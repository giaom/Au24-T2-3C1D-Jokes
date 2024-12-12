using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Joke;

namespace Joke.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor that passes options to the base DbContext class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Joke> Jokes { get; set; }
    }

}


