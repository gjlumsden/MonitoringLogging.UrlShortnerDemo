using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Training.UrlShortner.Functions.Functions;
using Training.UrlShortner.Functions.Persistence;

namespace Training.UrlShortner.Functions.Services
{
    public class AddAliasService : IAddAliasSerivce
    {
        public const string AddAliasQuery = "INSERT INTO aliases (alias, url) VALUES (@alias, @url)";

        private static Random Random = new Random();

        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<AddAliasService> _logger;

        public AddAliasService(ISqlConnectionFactory sqlConnectionFactory, TelemetryClient telemetryClient, ILogger<AddAliasService> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public async Task ExecuteAsync(AddAliasProperties requestContent, DateTime enqueuedTimeUtc)
        {
            var intentionalDelay = Random.Next(3000);
            await Task.Delay(intentionalDelay);

            _logger.LogInformation("Adding alias to database: {alias}", JsonConvert.SerializeObject(requestContent));
            using (var connection = await _sqlConnectionFactory.CreateConnectionAsync())
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
            var delayInMiliSeconds = (int) delay.TotalMilliseconds;
            _logger.LogTrace("Recording delay of {delay} ms in adding alias", delayInMiliSeconds);

            _telemetryClient.GetMetric("AddAliasProcessingDelay").TrackValue(delayInMiliSeconds);
        }
    }
}