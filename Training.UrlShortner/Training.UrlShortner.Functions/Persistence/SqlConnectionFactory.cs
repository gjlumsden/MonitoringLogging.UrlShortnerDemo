using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Training.UrlShortner.Functions.Configuration;

namespace Training.UrlShortner.Functions.Persistence
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        public const string CreateTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='aliases' and xtype='U')
        CREATE TABLE aliases (
            id INT NOT NULL IDENTITY(1,1),
            alias NVARCHAR(64) NOT NULL,
            url NVARCHAR(1024) NOT NULL
        )";

        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly IRuntimeConfiguration _runtimeConfiguration;

        private bool _databasesIsInitialized;

        public SqlConnectionFactory(IRuntimeConfiguration runtimeConfiguration)
        {
            _runtimeConfiguration = runtimeConfiguration;
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            await InitDatabaseIfNeededAsync();

            return CreateConnection();
        }

        private async Task InitDatabaseIfNeededAsync()
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

        private SqlConnection CreateConnection()
        {
            var connectionString = _runtimeConfiguration.GetString("Database.ConnectionString");
            return new SqlConnection(connectionString);
        }
    }
}