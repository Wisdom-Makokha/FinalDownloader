using FinalDownloader.Data.Interface;
using FinalDownloader.Models;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Data
{
    internal class DBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryRepository _categoryRepository;

        public DBInitializer(ApplicationDbContext context, ICategoryRepository categoryRepository)
        {
            _context = context;
            _categoryRepository = categoryRepository;
        }

        public async Task Initialize()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            await _context.Database.MigrateAsync();

            // Check if categories already exist
            if ((await _categoryRepository.GetAllAsync()).Any())
            {
                return; // Database has been seeded
            }

            // Seed initial categories
            var defaultCategory = new Category
            {
                Name = "General",
                FolderPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "General"),
                Type = Models.MediaMetadata.MediaType.VideoNAudio,
                Description = "Default category for all media types",
                Resolution = "1080p",
                Subtitles = false,
                IsDefault = true
            };

            await _categoryRepository.AddAsync(defaultCategory);
            _context.SaveChanges();
        }
    }
}
