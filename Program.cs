using ConsoleStackNavigation.NavigationSystem;
using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.NavigationSystem.Interface;
using Microsoft.Extensions.DependencyInjection;
using FinalDownloader.Display.Screens;
using FinalDownloader.Constants;
using System.Threading.Tasks;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Display;
using FinalDownloader.Display.Menus;
using FinalDownloader.Data;
using FinalDownloader.Services.Download;
using FinalDownloader.Services.Process;
using FinalDownloader.Services.Arguments;
using FinalDownloader.Models;
using FinalDownloader.Data.Interface;
using FinalDownloader.Data.Repository;
using FinalDownloader.Display.Screens.Category;
using FinalDownloader.Services.Update;
using Spectre.Console;
using FinalDownloader.Models.Settings;
using FinalDownloader.Display.Screens.Download;
using FinalDownloader.Services.Configuration;
using FinalDownloader.Display.Screens.Settings;
using FinalDownloader.Services.Parser;
using FinalDownloader.Services.MetadataRetrieval;
using FinalDownloader.Services.FileHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FinalDownloader.Services.Progress;

namespace FinalDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            { 
                Console.SetWindowSize(128, 29);
                //Console.
            }
            var services = new ServiceCollection();

            // Register services and dependencies here
            services.AddNavigation(options =>
            {
                options.ErrorScreenType = typeof(DefaultErrorScreen);
            });

            var connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build()
                .GetConnectionString("DefaultConnection");

            // add database context as a singleton
            services.AddDbContext<ApplicationDbContext>();
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // add singleton services
            services.AddSingleton<IMenuFactory, MenuFactory>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // add scoped services
            services.AddScoped<IDownloadService, DownloadService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IMediaContainerBaseRepository, MediaContainerBaseRepository>();
            services.AddScoped<IMediaMetadataBaseRepository, MediaMetadataRepository>();

            // add transient services
            services.AddTransient<IProcessService, ProcessService>();
            services.AddTransient<IArgumentService, ArgumentService>();
            services.AddTransient<IUpdateService, UpdateService>();
            services.AddTransient<IParserService, ParserService>();
            services.AddTransient<IMetadataService, MetadataService>();
            services.AddTransient<IFileHandlingService, FileHandlingService>();
            services.AddTransient<IDownloadService, DownloadService>();
            services.AddTransient<IProgressService, ProgressService>();
            services.AddTransient<DBInitializer>();

            // add transient screens
            services.AddTransient<WelcomeScreen>();
            services.AddTransient<ExitScreen>();
            services.AddTransient<DownloadScreen>();
            services.AddTransient<UpdateScreen>();
            services.AddTransient<CategoryDetailsScreen>();
            services.AddTransient<AddCategoryScreen>();
            services.AddTransient<EditCategoryScreen>();
            services.AddTransient<DeleteCategoryScreen>();
            services.AddTransient<DownloadSettingsDetailsScreen>();
            services.AddTransient<EditDownloadSettingsScreen>();
            services.AddTransient<ArgumentSettingsDetailsScreen>();
            services.AddTransient<EditArgumentSettingsScreen>();

            var serviceProvider = services.BuildServiceProvider();
            var navigationService = serviceProvider.GetRequiredService<INavigationService>();
            var registry = serviceProvider.GetRequiredService<IScreenRegistry>();
            var menuFactory = serviceProvider.GetRequiredService<IMenuFactory>();
            var dbInitializer = serviceProvider.GetRequiredService<DBInitializer>();
            var updateService = serviceProvider.GetRequiredService<IUpdateService>();

            await dbInitializer.Initialize();

            // Register screens with the navigation service
            registry.RegisterScreen(ScreenNames.WelcomeScreen, serviceProvider.GetRequiredService<WelcomeScreen>);
            registry.RegisterScreen(ScreenNames.ExitScreen, serviceProvider.GetRequiredService<ExitScreen>);
            registry.RegisterScreen(ScreenNames.DownloadScreen, serviceProvider.GetRequiredService<DownloadScreen>);
            registry.RegisterScreen(ScreenNames.UpdateScreen, serviceProvider.GetRequiredKeyedService<UpdateScreen>);
            registry.RegisterScreen(ScreenNames.CategoryDetailsScreen, serviceProvider.GetRequiredService<CategoryDetailsScreen>);
            registry.RegisterScreen(ScreenNames.AddCategoryScreen, serviceProvider.GetRequiredService<AddCategoryScreen>);
            registry.RegisterScreen(ScreenNames.EditCategoryScreen, serviceProvider.GetRequiredService<EditCategoryScreen>);
            registry.RegisterScreen(ScreenNames.DeleteCategoryScreen, serviceProvider.GetRequiredService<DeleteCategoryScreen>);
            registry.RegisterScreen(ScreenNames.DownloadSettingsDetailsScreen, serviceProvider.GetRequiredService<DownloadSettingsDetailsScreen>);
            registry.RegisterScreen(ScreenNames.EditDownloadSettingsScreen, serviceProvider.GetRequiredService<EditDownloadSettingsScreen>);
            registry.RegisterScreen(ScreenNames.ArgumentSettingsDetailsScreen, serviceProvider.GetRequiredService<ArgumentSettingsDetailsScreen>);
            registry.RegisterScreen(ScreenNames.EditArgumentSettingsScreen, serviceProvider.GetRequiredService<EditArgumentSettingsScreen>);

            // register menus with the navigation service
            registry.RegisterScreen(MenuNames.MainMenu, menuFactory.CreateMainMenu);
            registry.RegisterScreen(MenuNames.SettingsMenu, menuFactory.CreateSettingsMenu);
            registry.RegisterScreen(MenuNames.CategorySettingsMenu, menuFactory.CreateCategoryMenu);
            registry.RegisterScreen(MenuNames.CategoryListMenu, menuFactory.CreateCategoryListMenu);
            registry.RegisterScreen(MenuNames.DownloadSettingsManagementMenu, menuFactory.CreateDownloadSettingsManagementMenu);
            registry.RegisterScreen(MenuNames.ArgumentSettingsManagementMenu, menuFactory.CreateArgumentSettingsManagementMenu);
            registry.RegisterScreen(MenuNames.CategoryManagementMenu, (category) =>
            {
                if (category is not CategoryData categoryModel)
                {
                    throw new ArgumentException("Invalid category data provided for CategoryManagementMenu.");
                }

                return menuFactory.CreateCategoryManagementMenu(categoryModel);
            });

            // initial navigation stack setup
            navigationService.PushScreen(ScreenNames.ExitScreen);
            navigationService.PushScreen(MenuNames.MainMenu);

            // Start the application with the welcome screen
            await navigationService.StartSystem(
                ScreenNames.WelcomeScreen,
                new WelcomeScreenData
                {
                    Title = "Final Downloader",
                    WelcomeMessage = "Welcome to the Final Downloader!",
                    DeveloperName = "DevPass"
                });
        }
    }
}
