using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderMaestro.Services;
using OrderMaestro.State;
using Serilog;


var configBuilder = new ConfigurationBuilder();
ConfigSetup(configBuilder);
var config = configBuilder.Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config.GetSection("QueueManager:Url").Value, config.GetSection("QueueManager:VirtualHost").Value, h =>
                {
                    h.Username(config.GetSection("QueueManager:User").Value);
                    h.Password(config.GetSection("QueueManager:Password").Value);
                });
                cfg.ReceiveEndpoint(config.GetSection("Queues:InputQueue").Value, cfg =>
                {
                    cfg.ConfigureSaga<OrderProcessState>(context);
                });

            });
            bus.AddSagaStateMachine<OrderMaestoStateMachineNew, OrderProcessState>()
                .MongoDbRepository(repo =>
                {
                    repo.Connection = config.GetSection("MongoDB:Url").Value;
                    repo.DatabaseName = config.GetSection("MongoDB:Database").Value;

                    repo.CollectionName = config.GetSection("MongoDB:Collection").Value;
                });
        });
    })
    .ConfigureLogging(logginBulder =>
    {

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateLogger();

        logginBulder.AddSerilog(logger, dispose: true);
    })
   .Build();

await host.RunAsync();


static void ConfigSetup(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
}