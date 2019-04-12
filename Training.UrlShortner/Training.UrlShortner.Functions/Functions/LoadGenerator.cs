using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Build.Framework;
using Training.UrlShortner.Functions.Configuration;

namespace Training.UrlShortner.Functions.Functions
{
    public static class LoadGenerator
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly RuntimeConfiguration RuntimeConfiguration = new RuntimeConfiguration();

        [FunctionName("LoadGenerator")]
        public static async Task Run([TimerTrigger("* * * * * *")]TimerInfo myTimer, 
            ExecutionContext executionContext)
        {
            var baseUrl = RuntimeConfiguration.GetString("UrlShortner.BaseUrl");
            var alias = Guid.NewGuid().ToString();
            var url = "https://nos.nl";

            await HttpClient.PostAsJsonAsync($"{baseUrl}/api/UrlsShortner", new AddAliasProperties
            {
                Alias = alias,
                Url = url
            });

            var tasks = Enumerable.Range(0, 5)
                .Select(x => HttpClient.GetAsync($"{baseUrl}/api/UrlsShortner/?a={alias}"));

            await Task.WhenAll(tasks);
        }
    }
}
