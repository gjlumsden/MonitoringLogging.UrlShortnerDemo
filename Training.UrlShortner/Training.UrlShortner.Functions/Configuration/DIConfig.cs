using Autofac;
using AzureFunctions.Autofac.Configuration;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Training.UrlShortner.Functions.Persistence;
using Training.UrlShortner.Functions.Services;
using LogLevel = NLog.LogLevel;

namespace Training.UrlShortner.Functions.Configuration
{
    public class DIConfig
    {
        public DIConfig(string functionName)
        {
            DependencyInjection.Initialize((builder) =>
            {
                builder.RegisterType<GetAliasService>().As<IGetAliasService>();

                builder.RegisterType<RedisDatabaseFactory>().As<IRedisDatabaseFactory>().SingleInstance();
                builder.RegisterType<RuntimeConfiguration>().As<IRuntimeConfiguration>().SingleInstance();

                ConfigureLogging(builder);
            }, functionName);
        }

        private static void ConfigureLogging(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterType<NLogLoggerFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();

            var configuration = new LoggingConfiguration();

            var applicationInsightsTarget = new ApplicationInsightsTarget()
            {
                Name = "ApplicationInsights"
            };

            configuration.AddTarget(applicationInsightsTarget);

            configuration.AddRule(LogLevel.Warn, LogLevel.Fatal, applicationInsightsTarget, "Microsoft.*", true);
            configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, applicationInsightsTarget, "*", true);

            LogManager.Configuration = configuration;
        }
    }
}
