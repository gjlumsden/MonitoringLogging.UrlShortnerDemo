using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Training.UrlShortner.Functions.Persistence
{
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> CreateConnectionAsync();
    }
}