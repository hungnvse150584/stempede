using System.Linq.Expressions;

namespace DataAccess.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Synchronous methods
        IQueryable<T> GetAllQueryable(string includeProperties = "");
        T Get(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteById(int id);
        void Save();

        // Asynchronous methods
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
        Task<IEnumerable<T>> GetAllAsync(string includeProperties = "");
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
        Task SaveAsync();
    }
}

