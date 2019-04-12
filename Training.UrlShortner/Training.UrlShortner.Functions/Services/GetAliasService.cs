using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Persistence;

namespace Training.UrlShortner.Functions.Services
{
    internal class GetAliasService : IGetAliasService
    {
        public const string GetAliasQuery = "SELECT url FROM aliases WHERE alias = @alias";

        private readonly IRuntimeConfiguration _configuration;
        private readonly IRedisDatabaseFactory _redisDatabaseFactory;
        private readonly ILogger<GetAliasService> _logger;
        private readonly TelemetryClient _telemetryClient;

        public GetAliasService(IRuntimeConfiguration configuration, IRedisDatabaseFactory redisDatabaseFactory, ILogger<GetAliasService> logger, TelemetryClient telemetryClient)
        {
            _configuration = configuration;
            _redisDatabaseFactory = redisDatabaseFactory;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task<string> GetAliasAsync(string alias)
        {
            string result;

            _telemetryClient.GetMetric("MethodUsage", "Method").TrackValue(1, "GetAlias");

            _logger.LogInformation("Fetching alias");
            using (var sqlConnection = new SqlConnection(_configuration.GetString("Database.ConnectionString")))
            {
                using (var command = new SqlCommand(GetAliasQuery, sqlConnection))
                {
                    command.Parameters.AddWithValue("alias", alias);
                    await sqlConnection.OpenAsync();
                    result = (string)await command.ExecuteScalarAsync();
                }
            }

            _logger.LogDebug("Recording usage");
            var redisConnection = _redisDatabaseFactory.GetDatabase();
            await Task.WhenAll(
                redisConnection.HashIncrementAsync(alias, "requestCount"),
                redisConnection.HashSetAsync(alias, "lastRequest", DateTime.UtcNow.ToString("O")));

                return result;
        }
    }
}