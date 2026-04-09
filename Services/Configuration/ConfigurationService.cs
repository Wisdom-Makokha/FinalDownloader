using FinalDownloader.Models.Settings;
using Microsoft.Extensions.Configuration;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ArgumentSettings ArgumentSettings { get; private set; }
        private readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public ConfigurationService()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            DownloadSettings = LoadDownloadSettings();
            ArgumentSettings = LoadArgumentSettings();
        }

        public void Reload()
        {
            DownloadSettings = LoadDownloadSettings();
            ArgumentSettings = LoadArgumentSettings();
        }

        private DownloadSettings LoadDownloadSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var settings = config.GetSection("DownloadSettings").Get<DownloadSettings>()
                ?? throw new ArgumentNullException(nameof(DownloadSettings), "DownloadSettings configuration is missing");

            return settings;
        }

        private ArgumentSettings LoadArgumentSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var settings = config.GetSection("ArgumentSettings").Get<ArgumentSettings>()
                ?? throw new ArgumentNullException(nameof(ArgumentSettings), "ArgumentSettings configuration is missing");

            return settings;
        }

        public async Task SaveDownloadSettings(DownloadSettings downloadSettings)
        {
            var configJson = await File.ReadAllTextAsync(_configFilePath);
            var configNode = JsonNode.Parse(configJson) ?? throw new ArgumentException("Failed to parse Settings JSON");

            configNode["DownloadSettings"] = JsonSerializer.SerializeToNode(downloadSettings, _jsonSerializerOptions);

            var updatedConfigJson = configNode.ToJsonString(_jsonSerializerOptions);
            await File.WriteAllTextAsync(_configFilePath, updatedConfigJson);
        }

        public async Task SaveArgumentSettings(ArgumentSettings argumentSettings)
        {
            var configJson = await File.ReadAllTextAsync(_configFilePath);
            var configNode = JsonNode.Parse(configJson) ?? throw new ArgumentException("Failed to parse Settings JSON");

            configNode["ArgumentSettigns"] = JsonSerializer.SerializeToNode(argumentSettings, _jsonSerializerOptions);

            var updatedConfigJson = configNode.ToJsonString(_jsonSerializerOptions);
            await File.WriteAllTextAsync(_configFilePath, updatedConfigJson);
        }
    }
}
