using System.Linq.Expressions;
using ToDoApp.Domain.Models;

namespace ToDoApp.Infrastructure.Repository;

public interface IRepository<T> where T : BaseEntity
{
    Task<List<T>> ToListAsync();
    Task<T?> GetByIdAsync(long? id);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
    Task<List<T>?> GetAllWhereAsync(Expression<Func<T, bool>> predicate);
}