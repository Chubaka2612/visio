using Visio.Domain.Core;

namespace Visio.Data.Core.Db
{
    public interface IDbRepository<TKey, TEntity> where TEntity : IEntity<TKey>
    {
        Task<TEntity> CreateAsync(TEntity entity);

        Task DeleteAsync(TKey id);

        Task<TEntity> ReadAsync(TKey id);

        Task UpdateAsync(TEntity entity);
    }
}