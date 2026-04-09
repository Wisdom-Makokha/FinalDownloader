using ConsoleStackNavigation.Menu;
using FinalDownloader.Display.ScreenData;
using FinalDownloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Display
{
    internal interface IMenuFactory
    {
        Menu CreateMainMenu();
        Menu CreateSettingsMenu();
        Menu CreateCategoryMenu();
        Menu CreateCategoryListMenu();
        Menu CreateCategoryManagementMenu(CategoryData category);
        Menu CreateDownloadSettingsManagementMenu();
        Menu CreateArgumentSettingsManagementMenu();
    }
}
