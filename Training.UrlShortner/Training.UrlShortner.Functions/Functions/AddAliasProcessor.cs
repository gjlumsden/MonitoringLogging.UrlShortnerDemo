using System;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Services;

namespace Training.UrlShortner.Functions.Functions
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class AddAliasProcessor
    {
        [FunctionName("AddAliasProcessor")]
        public static async Task Run(
            [ServiceBusTrigger("%ServiceBus.AddAliasQueueName%", Connection = "ServiceBus.ConnectionString")]string queueMessageJson,
            DateTime enqueuedTimeUtc,
            [Inject] IAddAliasSerivce addAliasService)
        {
            var requestContent = JsonConvert.DeserializeObject<AddAliasProperties>(queueMessageJson);
            await addAliasService.ExecuteAsync(requestContent, enqueuedTimeUtc);
        }
    }
}
