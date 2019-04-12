using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Training.UrlShortner.Functions.Functions;
using Training.UrlShortner.Functions.Persistence;

namespace Training.UrlShortner.Functions.Services
{
    public class AddAliasService : IAddAliasSerivce
    {
        public const string AddAliasQuery = "INSERT INTO aliases (alias, url) VALUES (@alias, @url)";

        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly TelemetryClient _telemetryClient;

        public AddAliasService(ISqlConnectionFactory sqlConnectionFactory, TelemetryClient telemetryClient)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _telemetryClient = telemetryClient;
        }

        public async Task ExecuteAsync(AddAliasProperties requestContent, DateTime enqueuedTimeUtc)
        {
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
            _telemetryClient.GetMetric("AddAliasProcessingDelay").TrackValue(delay.TotalMilliseconds);
        }
    }
}