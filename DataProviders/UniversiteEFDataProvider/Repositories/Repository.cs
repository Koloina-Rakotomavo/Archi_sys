using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public abstract class Repository<T>(UniversiteDbContext context) : IRepository<T> where T : class
{
    protected readonly UniversiteDbContext Context = context;
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual async Task<T> CreateAsync(T entity)
    {
        await Set.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        Set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(long id)
    {
        var entity = await FindAsync(id);
        if (entity is null)
            return;

        await DeleteAsync(entity);
    }

    public virtual Task DeleteAsync(T entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<T?> FindAsync(long id) =>
        await Set.FindAsync(id);

    public virtual async Task<T?> FindAsync(params object[] keyValues) =>
        await Set.FindAsync(keyValues);

    public virtual async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> condition) =>
        await Set.Where(condition).ToListAsync();

    public virtual async Task<List<T>> FindAllAsync() =>
        await Set.ToListAsync();

    public virtual async Task SaveChangesAsync() =>
        await Context.SaveChangesAsync();
}
