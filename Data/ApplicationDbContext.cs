using Microsoft.EntityFrameworkCore;
using FinalDownloader.Models;
using FinalDownloader.Models.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FinalDownloader.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MediaMetadataBase> MediaMetadata { get; set; }
        public DbSet<MediaContainerBase> MediaContainers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var configSection = configBuilder.GetSection("ConnectionStrings");
            var connectionString = configSection.GetValue<string>("DefaultConnection");

            if(string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not defined in appsettings.json.");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(c =>
            {
                c.HasKey(c => c.Id);

                c.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                c.HasIndex(c => c.Name)
                    .IsUnique();

                c.Property(c => c.FolderPath)
                    .IsRequired()
                    .HasMaxLength(500);

                c.HasIndex(c => c.FolderPath)
                    .IsUnique();

                c.HasMany(c => c.Items)
                    .WithOne(m => m.Category)
                    .HasForeignKey(m => m.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            modelBuilder.Entity<MediaContainerBase>(m =>
            {
                m.HasKey(m => m.UniqueId);

                m.Property(m => m.Title)
                    .IsRequired()
                    .HasMaxLength(500);

                m.Property(m => m.Url)
                    .IsRequired()
                    .HasMaxLength(1000);

                m.Property(m => m.Album)
                    .HasMaxLength(200);

                m.Property(m => m.Artist)
                    .HasMaxLength(200);

                m.Property(m => m.Artists)
                    .HasMaxLength(1000);

                m.HasMany(m => m.Items)
                    .WithOne(i => i.MediaContainerBase)
                    .HasForeignKey(i => i.MediaContainerBaseId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<MediaMetadataBase>(m =>
            {
                m.HasKey(m => m.Id);

                m.Property(m => m.Title)
                    .IsRequired()
                    .HasMaxLength(500);

                m.Property(m => m.Url)
                    .IsRequired()
                    .HasMaxLength(1000);

                m.Property(m => m.Uploader)
                    .HasMaxLength(200);

                m.Property(m => m.UploadDate)
                    .HasMaxLength(8);
            });
        }
    }
}
