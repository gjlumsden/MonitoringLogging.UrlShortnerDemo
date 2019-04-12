using System.Threading.Tasks;

namespace Training.UrlShortner.Functions.Services
{
    public interface IGetAliasService
    {
        Task<string> GetAliasAsync(string alias);
    }
}
