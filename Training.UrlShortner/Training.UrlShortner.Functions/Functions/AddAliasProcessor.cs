using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Training.UrlShortner.Functions.Functions
{
    public static class AddAliasProcessor
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();

        public const string AddAliasQuery = "INSERT INTO aliases (alias, url) VALUES (@alias, @url)";

        private static IConfigurationRoot _configuration;
        private static readonly object ConfigurationLock = new object();

        [FunctionName("AddAliasProcessor")]
        public static async Task Run(
            [ServiceBusTrigger("%ServiceBus.AddAliasQueueName%", Connection = "ServiceBus.ConnectionString")]string queueMessageJson,
            DateTime enqueuedTimeUtc,
            ExecutionContext executionContext)
        {
            InitConfigurationIfNeeded(executionContext);

            var requestContent = JsonConvert.DeserializeObject<AddAliasProperties>(queueMessageJson);

            using (var connection = CreateConnection())
            {
                using (var command = new SqlCommand(AddAliasQuery, connection))
                {
                    command.Parameters.AddWithValue("url", requestContent.Url);
                    command.Parameters.AddWithValue("alias", requestContent.Alias);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }

            var delay = DateTime.UtcNow - enqueuedTimeUtc;
            TelemetryClient.GetMetric("AddAliasProcessingDelay").TrackValue(delay.TotalMilliseconds);
        }

        private static void InitConfigurationIfNeeded(ExecutionContext context)
        {
            if (_configuration != null)
            {
                return;
            }

            lock (ConfigurationLock)
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                        .SetBasePath(context.FunctionAppDirectory)
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
                }
            }
        }

        private static SqlConnection CreateConnection()
        {
            var connectionString = _configuration["Database.ConnectionString"];
            return new SqlConnection(connectionString);
        }
    }
}
