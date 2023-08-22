using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Example.Database;

// dotnet ef migrations add InitialCreate --output-dir Database/Migrations
// dotnet ef database update
public class ExampleDbContext : DbContext
{
    public DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=example;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}

public abstract record Entity(DateTimeOffset CreatedAt)
{
    public Guid Id { get; set; }
}

public record User(string ExternalId, string ConnectionName, string? Email) : Entity(DateTimeOffset.UtcNow);
