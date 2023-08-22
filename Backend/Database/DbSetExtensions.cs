using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Example.Database
{
    public static class DbSetExtensions
    {
        public static EntityEntry<T>? AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>>? predicate = null) where T : Entity
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }

        public static void EnsureDoesNotExist<T>(this DbSet<T> dbSet, Expression<Func<T, bool>>? predicate = null) where T : Entity
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            if (exists)
            {
                throw new InvalidOperationException($"Entity of type '{typeof(T).Name}' already exists.");
            }
        }
    }
}
