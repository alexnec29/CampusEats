namespace CampusEats.Api.Infrastructure.Repositories;

// Do not change!!
// How to use:
// Create a new interface that extends this one, and add methods there
// T is the type of entity and TK is the type of id the T entity uses
public interface IRepository<T, TK>
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(TK id);
    Task<IList<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task DeleteAsync(T id);
}