using StackExchange.Redis;

namespace Training.UrlShortner.Functions.Persistence
{
    public interface IRedisDatabaseFactory
    {
        IDatabase GetDatabase();
    }
}