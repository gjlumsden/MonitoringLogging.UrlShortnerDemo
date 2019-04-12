using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;


namespace Training.UrlShortner.Functions.Functions
{
    public static class CleanUp
    {
        public const string CleanUpQuery = @"TRUNCATE TABLE aliases";

        private static IConfigurationRoot _configuration;
        private static readonly object ConfigurationLock = new object();

        [FunctionName("CleanUp")]
        public static async Task Run([TimerTrigger("0 5 */6 * * *")]TimerInfo myTimer, ExecutionContext executionContext)
        {
            InitConfigurationIfNeeded(executionContext);

            using (var connection = CreateConnection())
            {
                using (var command = new SqlCommand(CleanUpQuery, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    connection.Close();
                }
            }
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
