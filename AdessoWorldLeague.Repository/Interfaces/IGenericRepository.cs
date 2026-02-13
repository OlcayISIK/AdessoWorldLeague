using System.Linq.Expressions;
using AdessoWorldLeague.Core.Entities;

namespace AdessoWorldLeague.Repository.Interfaces;

public interface IGenericRepository<T> where T : BaseDocument
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> filter);
    Task CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
    Task HardDeleteAsync(string id);
}
