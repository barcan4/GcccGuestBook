using Microsoft.Extensions.Configuration;

namespace GcccData
{
    public class AppSettings
    {
        public string StorageConnectionString { get; set; }
        public static AppSettings LoadAppSettings()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("Settings.json")
                .Build();
            AppSettings appSettings = configuration.Get<AppSettings>();
            return appSettings;

        }
    }
}
