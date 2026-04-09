using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Interface
{
    internal interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}
