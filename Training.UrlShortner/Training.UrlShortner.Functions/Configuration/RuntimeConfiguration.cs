using System;
using Microsoft.Extensions.Configuration;

namespace Training.UrlShortner.Functions.Configuration
{
    public class RuntimeConfiguration : IRuntimeConfiguration
    {
        private readonly IConfiguration _configuration;

        public RuntimeConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string GetString(string settingName)
        {
            return _configuration[settingName];
        }
    }
}