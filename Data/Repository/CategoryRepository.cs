using FinalDownloader.Data.Interface;
using FinalDownloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Repository
{
    internal class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await Task.Run(() => _dbSet.FirstOrDefault(c => c.Name == name));
        }

        public async Task<IEnumerable<string>> GetAllCategoryNamesAsync()
        {
            return await Task.Run(() => _dbSet.Select(c => c.Name).ToList());
        }

        public async Task<Category?> GetDefaultCategoryAsync()
        {
            return await Task.Run(() => _dbSet.FirstOrDefault(c => c.IsDefault));
        }

        public async Task<bool> SetDefaultCategoryAsync(string name, CancellationToken cancellationToken = default)
        {
            var categoryToSet = await GetByNameAsync(name);
            if (categoryToSet == null)
                return false;

            // Unset the current default category
            var currentDefault = await GetDefaultCategoryAsync();
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                _context.Update(currentDefault);
            }
            
            // Set the new default category
            categoryToSet.IsDefault = true;
            
            _context.Update(categoryToSet);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
