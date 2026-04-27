using ConsoleStackNavigation.BaseScreens;
using ConsoleStackNavigation.Menu;
using ConsoleStackNavigation.NavigationSystem;
using FinalDownloader.Constants;
using FinalDownloader.Data.Interface;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display.Menus
{
    internal class MenuFactory : IMenuFactory
    {
        private readonly ICategoryRepository _categoryRepository;

        public MenuFactory(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Menu CreateMainMenu()
        {
            var options = new HashSet<MenuOption>()
            {
                new MenuOption(
                    title: "Download",
                    description: "Go to the download menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.DownloadScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title : "Settings",
                    description: "Go to the settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.SettingsMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title : "Merge Records",
                    description: "Go to the records merge screen",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.RecordsMergeScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title : "Exit",
                    description: "Exit the application",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.ExitScreen,
                        ScreenAction = NavigationAction.Clear,
                        Data = new ExitScreenData
                        {
                            Title = "Exit Final Downloader",
                            ExitMessage = "Closing application... "
                        },
                        MenuScreenCustomizationData = null
                    }
                )
            };

            return new Menu(
                title: MenuNames.MainMenu,
                menuPrompt: "Please select an option:",
                options: options,
                data: null
            );
        }
        #region Settings
        public Menu CreateSettingsMenu()
        {
            var options = new HashSet<MenuOption>()
            {
                new MenuOption(
                    title: "Category Settings",
                    description: "Go to the category settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.CategorySettingsMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Download Settings",
                    description: "Go to the download settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.DownloadSettingsManagementMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Argument Settings",
                    description: "Go to the argument settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.ArgumentSettingsManagementMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Update Tools",
                    description: "Update yt-dlp and any other tools added to the application",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.UpdateScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "BACK",
                    description: "Go back to the main menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.MainMenu,
                        ScreenAction = NavigationAction.Pop,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                )
            };

            return new Menu(
                title: MenuNames.SettingsMenu,
                options: options,
                data: null);
        }
        #endregion

        #region Category
        public Menu CreateCategoryMenu()
        {
            var options = new HashSet<MenuOption>()
            {
                new MenuOption(
                    title: "Show Categories",
                    description: "Lists the categories and allows for further options per category",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.CategoryListMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Add Category",
                    description: "Add a new category to the application",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.AddCategoryScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "BACK",
                    description: "Go back to the settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.SettingsMenu,
                        ScreenAction = NavigationAction.Pop,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                )
            };
            return new Menu(
                title: MenuNames.CategorySettingsMenu,
                options: options,
                data: null);
        }

        public Menu CreateCategoryListMenu()
        {
            var categories = _categoryRepository.GetAllCategoryNamesAsync().Result.ToList();

            var options = new HashSet<MenuOption>();
            foreach (var category in categories)
            {
                options.Add(new MenuOption(
                    title: category,
                    description: $"Manage {category} category",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.CategoryManagementMenu,
                        ScreenAction = NavigationAction.Push,
                        Data = new CategoryData { Name = category },
                        MenuScreenCustomizationData = new CategoryData { Name = category }
                    }
                ));
            }

            options.Add(new MenuOption(
                title: "BACK",
                description: "Go back to the category menu",
                action: (data) => new NavigationResult()
                {
                    NextScreenKey = MenuNames.CategoryListMenu,
                    ScreenAction = NavigationAction.Pop,
                    Data = null
                }
            ));

            return new Menu(
                title: MenuNames.CategoryListMenu,
                options: options,
                data: null);
        }

        public Menu CreateCategoryManagementMenu(CategoryData category)
        {
            var options = new HashSet<MenuOption>()
            {
                new MenuOption(
                    title: "View Category Details",
                    description: $"View details about the {category.Name} category",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.CategoryDetailsScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = category
                    }
                ),
                new MenuOption(
                    title: "Edit Category",
                    description: $"Edit the {category.Name} category",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.EditCategoryScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = category
                    }
                ),
                new MenuOption(
                    title: "Set Default Category",
                    description: $"Set the {category.Name} category as the default category for new downloads",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.SetDefaultCategoryScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = category
                    }
                ),
                new MenuOption(
                    title: "Delete Category",
                    description: $"Delete the {category.Name} category",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.DeleteCategoryScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = category
                    }
                ),
                new MenuOption(
                    title: "BACK",
                    description: "Go back to the category list menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.CategoryListMenu,
                        ScreenAction = NavigationAction.Pop,
                        Data = null
                    }
                )
            };

            return new Menu(
                title: $"{category.Name} - Category Settings",
                options: options,
                data: category);
        }
        #endregion

        #region Download Settings
        public Menu CreateDownloadSettingsManagementMenu()
        {
            var options = new HashSet<MenuOption>()
            {
                new MenuOption(
                    title: "View Current Settings",
                    description: "Get details on the currently selected options",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.DownloadSettingsDetailsScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Modify Download Settings",
                    description: "Change settings to preferred values",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.EditDownloadSettingsScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "BACK",
                    description: "Go back to the settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.SettingsMenu,
                        ScreenAction = NavigationAction.Pop,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                )
            };
            return new Menu(
                title: MenuNames.DownloadSettingsManagementMenu,
                options: options,
                data: null);
        }
        #endregion

        #region Argument Settings
        public Menu CreateArgumentSettingsManagementMenu()
        {
            var options = new HashSet<MenuOption>()
                {
                new MenuOption(
                    title: "View Current Arguments",
                    description: "Get details on the currently selected arguments",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.ArgumentSettingsDetailsScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "Modify Arguments",
                    description: "Change arguments to preferred values",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = ScreenNames.EditArgumentSettingsScreen,
                        ScreenAction = NavigationAction.Push,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                ),
                new MenuOption(
                    title: "BACK",
                    description: "Go back to the settings menu",
                    action: (data) => new NavigationResult()
                    {
                        NextScreenKey = MenuNames.SettingsMenu,
                        ScreenAction = NavigationAction.Pop,
                        Data = null,
                        MenuScreenCustomizationData = null
                    }
                )
            };

            return new Menu(
                title: MenuNames.ArgumentSettingsManagementMenu,
                options: options,
                data: null);
        }
        #endregion
    }
}
