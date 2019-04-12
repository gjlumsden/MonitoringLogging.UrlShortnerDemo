using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Services;

namespace Training.UrlShortner.Functions.Functions
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class LoadGenerator
    {
        [FunctionName("LoadGenerator")]
        public static async Task Run([TimerTrigger("* * * * * *")]TimerInfo myTimer, 
            [Inject] ILoadGeneratorService loadGeneratorService)
        {
            await loadGeneratorService.ExecuteAsync();
        }
    }
}
