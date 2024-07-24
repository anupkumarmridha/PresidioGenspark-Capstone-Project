namespace NewsAppAPI.Repositories.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(K id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(K id);
    }
}
