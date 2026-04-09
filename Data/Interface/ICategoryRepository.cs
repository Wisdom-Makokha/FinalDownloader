using FinalDownloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data.Interface
{
    internal interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<IEnumerable<string>> GetAllCategoryNamesAsync();
        Task<Category?> GetDefaultCategoryAsync();
        Task<bool> SetDefaultCategoryAsync(string name, CancellationToken cancellationToken = default);
    }
}
