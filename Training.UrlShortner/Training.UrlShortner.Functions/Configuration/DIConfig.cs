using Autofac;
using AzureFunctions.Autofac.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Seq.Client.NLog;
using StackExchange.Redis;
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
                var runtimeConfiguration = new RuntimeConfiguration();
                builder.RegisterInstance(runtimeConfiguration).As<IRuntimeConfiguration>().SingleInstance();

                ConfigureLogging(runtimeConfiguration, builder);
                ConfigureMonitoring(builder);

                builder.RegisterType<RedisDatabaseFactory>().As<IRedisDatabaseFactory>().SingleInstance();
                builder.RegisterType<SqlConnectionFactory>().As<ISqlConnectionFactory>().SingleInstance();

                builder.RegisterType<CleanUpService>().As<ICleanUpService>().InstancePerLifetimeScope();
                builder.RegisterType<LoadGeneratorService>().As<ILoadGeneratorService>().InstancePerLifetimeScope();
                builder.RegisterType<AddAliasService>().As<IAddAliasSerivce>().InstancePerLifetimeScope();
                builder.RegisterType<GetAliasService>().As<IGetAliasService>().InstancePerLifetimeScope();
            }, functionName);
        }

        private void ConfigureLogging(RuntimeConfiguration runtimeConfiguration, ContainerBuilder builder)
        {
            var configuration = new LoggingConfiguration();

            Target loggingTarget;
            if (runtimeConfiguration.GetBool("UseSeq"))
            {
                var seqTarget = new SeqTarget
                {
                    Name = "Seq",
                    ServerUrl = "http://localhost:5341",
                    Properties =
                    {
                        new SeqPropertyItem
                        {
                            Name = "MachineName",
                            Value = "localhost",
                        },
                        new SeqPropertyItem
                        {
                            Name = "hostname",
                            Value = "${hostname}",
                        },
                        new SeqPropertyItem
                        {
                            Name = "source",
                            Value = "${callsite:fileName=true}",
                        }
                    },
                };

                loggingTarget = new BufferingTargetWrapper
                {
                    Name = "buffer",
                    BufferSize = 1000,
                    FlushTimeout = 2500,
                    WrappedTarget = seqTarget
                };
            }
            else
            {
                loggingTarget = new ApplicationInsightsTarget()
                {
                    Name = "ApplicationInsights"
                };
            }

            configuration.AddTarget(loggingTarget);

            configuration.AddRule(LogLevel.Warn, LogLevel.Fatal, loggingTarget, "Microsoft.*", true);
            configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, loggingTarget, "*", true);

            LogManager.Configuration = configuration;
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterType<NLogLoggerFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }

        private void ConfigureMonitoring(ContainerBuilder builder)
        {
            builder.RegisterType<TelemetryClient>().SingleInstance();
        }
    }
}
