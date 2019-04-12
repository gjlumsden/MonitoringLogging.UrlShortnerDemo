using System.Threading.Tasks;
using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Training.UrlShortner.Functions.Configuration;
using Training.UrlShortner.Functions.Services;

namespace Training.UrlShortner.Functions.Functions
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class CleanUp
    {
        [FunctionName("CleanUp")]
        public static async Task Run(
            [TimerTrigger("0 5 */6 * * *")]TimerInfo myTimer,
            [Inject] ICleanUpService cleanUpService)
        {
            await cleanUpService.ExecuteAsync();
        }
    }
}
