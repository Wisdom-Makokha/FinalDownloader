using FinalDownloader.Models.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Configuration
{
    internal class ConfigurationService : IConfigurationService
    {
        public DownloadSettings DownloadSettings { get; private set; }

        public ArgumentSettings ArgumentSettings { get; private set; }
        private readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public ConfigurationService()
        {
            DownloadSettings = LoadDownloadSettings();
            ArgumentSettings = LoadArgumentSettings();
        }

        public void Reload()
        {
            DownloadSettings = LoadDownloadSettings();
            ArgumentSettings = LoadArgumentSettings();
        }

        private IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private DownloadSettings LoadDownloadSettings()
        {
            var config = BuildConfiguration();

            var settings = config.GetSection("DownloadSettings").Get<DownloadSettings>()
                ?? throw new ArgumentNullException(nameof(DownloadSettings), "DownloadSettings configuration is missing");

            return settings;
        }

        private ArgumentSettings LoadArgumentSettings()
        {
            var config = BuildConfiguration();

            var settings = config.GetSection("ArgumentSettings").Get<ArgumentSettings>()
                ?? throw new ArgumentNullException(nameof(ArgumentSettings), "ArgumentSettings configuration is missing");

            return settings;
        }

        private async Task<string> ReadConfigFileAsync()
        {
            if (!File.Exists(_configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found", _configFilePath);
            }
            return await File.ReadAllTextAsync(_configFilePath);
        }

        public async Task SaveDownloadSettings(DownloadSettings downloadSettings)
        {
            try
            {
                var configJson = await ReadConfigFileAsync();
                var configObj = JObject.Parse(configJson);

                configObj["DownloadSettings"] = JObject.FromObject(downloadSettings);

                var updatedConfigJson = configObj.ToString(Formatting.Indented);
                await File.WriteAllTextAsync(_configFilePath, updatedConfigJson);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException("Failed to save DownloadSettings due to insufficient permissions.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to save DownloadSettings due to an I/O error.", ex);
            }
        }

        public async Task SaveArgumentSettings(ArgumentSettings argumentSettings)
        {
            try
            {
                var configJson = await ReadConfigFileAsync();
                var configObj = JObject.Parse(configJson);

                configObj["ArgumentSettings"] = JObject.FromObject(argumentSettings);

                var updatedConfigJson = configObj.ToString(Formatting.Indented);
                await File.WriteAllTextAsync(_configFilePath, updatedConfigJson);
            }
            catch (UnauthorizedAccessException ex) {
                throw new InvalidOperationException("Failed to save ArgumentSettings due to insufficient permissions.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to save ArgumentSettings due to an I/O error.", ex);
            }
        }
    }
}
