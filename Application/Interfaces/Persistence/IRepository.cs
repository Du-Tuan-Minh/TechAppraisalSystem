using System.Linq.Expressions;

namespace Application.Interfaces.Persistence
{
    public interface IRepository<T> where T : class
    {
        // --- READ 
        Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includeProperties);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includeProperties);

        // IQueryable để phân trang (Pagination) hoặc lọc 
        IQueryable<T> GetAllAsQueryable();
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);

        // Kiểm tra nhanh sự tồn tại
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // --- WRITE
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        // --- UPDATE & DELETE 
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}