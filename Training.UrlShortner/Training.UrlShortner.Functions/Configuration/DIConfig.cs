using Autofac;
using AzureFunctions.Autofac.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Training.UrlShortner.Functions.Functions;
using Training.UrlShortner.Functions.Persistence;
using Training.UrlShortner.Functions.Services;
using LogLevel = NLog.LogLevel;

namespace Training.UrlShortner.Functions.Configuration
{
    public class DiConfig
    {
        public DiConfig(string functionName)
        {
            DependencyInjection.Initialize((builder) =>
            {
                ConfigureLogging(builder);
                ConfigureMonitoring(builder);

                builder.RegisterType<RuntimeConfiguration>().As<IRuntimeConfiguration>().SingleInstance();

                builder.RegisterType<RedisDatabaseFactory>().As<IRedisDatabaseFactory>().SingleInstance();
                builder.RegisterType<SqlConnectionFactory>().As<ISqlConnectionFactory>().SingleInstance();

                builder.RegisterType<CleanUpService>().As<ICleanUpService>().InstancePerLifetimeScope();
                builder.RegisterType<LoadGeneratorService>().As<ILoadGeneratorService>().InstancePerLifetimeScope();
                builder.RegisterType<AddAliasService>().As<IAddAliasSerivce>().InstancePerLifetimeScope();
                builder.RegisterType<GetAliasService>().As<IGetAliasService>().InstancePerLifetimeScope();
            }, functionName);
        }

        private void ConfigureLogging(ContainerBuilder builder)
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

        private void ConfigureMonitoring(ContainerBuilder builder)
        {
            builder.RegisterType<TelemetryClient>().SingleInstance();
        }
    }
}
