using Example.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Example.Database;

// dotnet ef migrations add InitialCreate --output-dir Database/Migrations
// dotnet ef database update
public class ExampleDbContext : DbContext
{
    public DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=example;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}

public class AuditInterceptor : ISaveChangesInterceptor
{
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditProperties(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditProperties(eventData.Context);
        return result;
    }

    public void AddAuditProperties(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var insertedEntries = context.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity);
        foreach (var entry in insertedEntries)
        {
            if (entry is Entity entity)
            {
                entity.CreatedAt = DateTimeOffset.UtcNow;
            }
        }

        var modifiedEntries = context.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Modified)
            .Select(x => x.Entity);
        foreach (var entry in modifiedEntries)
        {
            if (entry is Entity entity)
            {
                entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}


public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
