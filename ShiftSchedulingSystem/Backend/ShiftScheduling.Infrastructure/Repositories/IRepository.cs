using System.Linq.Expressions;

namespace ShiftScheduling.Infrastructure.Repositories
{
    public interface IRepository<T> where T : class
    {
        // Get entity by its primary key
        Task<T?> GetByIdAsync(int id);
        
        // Get all entities
        Task<IEnumerable<T>> GetAllAsync();
        
        // Find entities matching a condition
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Add a new entity
        Task<T> AddAsync(T entity);
        
        // Update an existing entity
        Task UpdateAsync(T entity);
        
        // Delete an entity
        Task DeleteAsync(T entity);
        
        // Check if any entity matches condition
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        
        // Count entities matching condition
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}