using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Quotes;

namespace Quotes.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Joke> Jokes { get; set; }
    }

}


