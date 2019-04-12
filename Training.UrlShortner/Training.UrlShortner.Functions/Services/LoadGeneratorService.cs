using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Functions;

namespace Training.UrlShortner.Functions.Services
{
    public class LoadGeneratorService : ILoadGeneratorService
    {
        private readonly IRuntimeConfiguration _runtimeConfiguration;

        private static readonly HttpClient HttpClient = new HttpClient();

        public LoadGeneratorService(IRuntimeConfiguration runtimeConfiguration)
        {
            _runtimeConfiguration = runtimeConfiguration;
        }

        public async Task ExecuteAsync()
        {
            var baseUrl = _runtimeConfiguration.GetString("UrlShortner.BaseUrl");
            var alias = Guid.NewGuid().ToString();
            var url = "https://null.null/null";

            await HttpClient.PostAsJsonAsync($"{baseUrl}/api/UrlsShortner", new AddAliasProperties
            {
                Alias = alias,
                Url = url
            });

            var tasks = Enumerable.Range(0, 5)
                .Select(x => HttpClient.GetAsync($"{baseUrl}/api/UrlsShortner/?a={alias}"));

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // I am redirecting to a non-existend host so this is expected
            }
        }
    }
}