using System.Data.SqlClient;
using System.Threading.Tasks;
using Training.UrlShortner.Functions.Persistence;

namespace Training.UrlShortner.Functions.Services
{
    internal class CleanUpService : ICleanUpService
    {
        public const string CleanUpQuery = @"TRUNCATE TABLE aliases";

        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public CleanUpService(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task ExecuteAsync()
        {
            using (var connection = await _sqlConnectionFactory.CreateConnectionAsync())
            {
                using (var command = new SqlCommand(CleanUpQuery, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    connection.Close();
                }
            }

        }
    }
}