using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Services;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Training.UrlShortner.Functions.Functions
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class UrlsShortner
    {
        public const string CreateTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='aliases' and xtype='U')
        CREATE TABLE aliases (
            id INT NOT NULL IDENTITY(1,1),
            alias NVARCHAR(64) NOT NULL,
            url NVARCHAR(1024) NOT NULL
        )";

        private static bool _databasesIsInitialized = false;
        private static readonly Semaphore _semaphore = new Semaphore(1,1);
        private static IConfigurationRoot _configuration;
        private static readonly object ConfigurationLock = new object();

        [FunctionName("UrlsShortner")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [ServiceBus("%ServiceBus.AddAliasQueueName%", Connection = "ServiceBus.ConnectionString")] ICollector<string> messageBusCollector,
            ExecutionContext executionContext,
            [Inject] IGetAliasService getAliasService)
        {
            InitConfigurationIfNeeded(executionContext);
            await InitDatabaseIfNeededAsync();

            if (req.Method == "POST")
            {
                var requestContentJson = await req.ReadAsStringAsync();
                var requestContent = JsonConvert.DeserializeObject<AddAliasProperties>(requestContentJson);
                if (string.IsNullOrWhiteSpace(requestContent.Alias) || string.IsNullOrWhiteSpace(requestContent.Url))
                {
                    return new BadRequestResult();
                }

                messageBusCollector.Add(requestContentJson);
                return new AcceptedResult();
            }

            if (!req.Query.ContainsKey("a"))
            {
                return new BadRequestResult();                
            }

            var result = await getAliasService.GetAliasAsync(req.Query["a"].First());
            return new RedirectResult(result);
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

        private static async Task InitDatabaseIfNeededAsync()
        {
            if (_databasesIsInitialized)
            {
                return;
            }

            using (var connection = CreateConnection())
            {
                _semaphore.WaitOne();
                if (_databasesIsInitialized)
                {
                    return;
                }

                using (var command = new SqlCommand(CreateTableQuery, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    connection.Close();
                }

                _databasesIsInitialized = true;
                _semaphore.Release(1);
            }
        }

        private static SqlConnection CreateConnection()
        {
            var connectionString = _configuration["Database.ConnectionString"];
            return new SqlConnection(connectionString);
        }
    }
}
