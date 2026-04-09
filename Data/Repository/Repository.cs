using FinalDownloader.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Repository
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _dbSet.AddAsync(entity);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result.Entity;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = _dbSet.Update(entity);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result.Entity;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var entity = await GetByIdAsync(id);

                if (entity == null) return false;

                _dbSet.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var entity = await GetByIdAsync(id);

                if (entity == null) return false;
                _dbSet.Remove(entity);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var entity = await GetByIdAsync(id);

            return entity != null;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var entity = await GetByIdAsync(id);

            return entity != null;
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
                {
                await _dbSet.AddRangeAsync(entities, cancellationToken);
                var result = await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
                {
                _dbSet.UpdateRange(entities);
                var result = await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<int> BulkDeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var entities = await _dbSet
                    .Where(e => ids.Contains(e.ToString())).ToListAsync(cancellationToken);

                _dbSet.RemoveRange(entities);
                
                var result = await _context.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<int> BulkDeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var idSet = new HashSet<string>(ids.Select(id => id.ToString()));

            try
            {
                var entities = await _dbSet
                    .Where(e => idSet.Contains(e.ToString()!)).ToListAsync(cancellationToken);

                _dbSet.RemoveRange(entities);

                var result = await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
