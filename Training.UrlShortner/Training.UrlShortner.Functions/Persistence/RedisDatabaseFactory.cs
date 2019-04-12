using System;
using StackExchange.Redis;
using Training.UrlShortner.Functions.Configuration;

namespace Training.UrlShortner.Functions.Persistence
{
    internal class RedisDatabaseFactory : IRedisDatabaseFactory, IDisposable
    {
        private readonly object _multiplexerLock = new object();
        private readonly IRuntimeConfiguration _runtimeConfiguration;
        private ConnectionMultiplexer _multiplexer;

        public RedisDatabaseFactory(IRuntimeConfiguration runtimeConfiguration)
        {
            _runtimeConfiguration = runtimeConfiguration;
        }

        public IDatabase GetDatabase()
        {
            // Note: locking is not like amazing, but still better than doing work in the constructor (especially work over the network)
            if (_multiplexer == null)
            {
                lock (_multiplexerLock)
                {
                    if (_multiplexer == null)
                    {
                        var connectionString = _runtimeConfiguration.GetString("Redis.ConnectionString");
                        _multiplexer = ConnectionMultiplexer.Connect(connectionString);
                    }
                }
            }

            return _multiplexer.GetDatabase();
        }

        public void Dispose()
        {
            _multiplexer?.Dispose();
        }
    }
}