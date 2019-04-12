using System.Linq;
using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Services;

namespace Training.UrlShortner.Functions.Functions
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class UrlsShortner
    {
        [FunctionName("UrlsShortner")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [ServiceBus("%ServiceBus.AddAliasQueueName%", Connection = "ServiceBus.ConnectionString")] ICollector<string> messageBusCollector,
            [Inject] IGetAliasService getAliasService)
        {
            if (req.Method == "POST")
            {
                var requestContentJson = await req.ReadAsStringAsync();
                var requestContent = JsonConvert.DeserializeObject<AddAliasProperties>(requestContentJson);
                if (string.IsNullOrWhiteSpace(requestContent.Alias) || string.IsNullOrWhiteSpace(requestContent.Url))
                {
                    return new BadRequestResult();
                }

                messageBusCollector.Add(requestContentJson);
                return new AcceptedResult();
            }

            if (!req.Query.ContainsKey("a"))
            {
                return new BadRequestResult();                
            }

            var result = await getAliasService.GetAliasAsync(req.Query["a"].First());

            if (result == null)
            {
                return new NotFoundResult();
            }

            return new RedirectResult(result);
        }
    }
}
