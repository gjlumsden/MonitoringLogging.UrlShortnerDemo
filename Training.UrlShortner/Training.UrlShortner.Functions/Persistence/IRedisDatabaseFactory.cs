using StackExchange.Redis;

namespace Training.UrlShortner.Functions.Persistence
{
    internal interface IRedisDatabaseFactory
    {
        IDatabase GetDatabase();
    }
}