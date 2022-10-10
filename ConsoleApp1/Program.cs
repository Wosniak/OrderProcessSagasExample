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
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");

                });
                cfg.ReceiveEndpoint(config.GetSection("Queues:InputQueue").Value, cfg =>
                {
                    cfg.ConfigureSaga<OrderProcessState>(context);
                });

            });
            bus.AddSagaStateMachine<OrderMaestoStateMachine, OrderProcessState>()
                .MongoDbRepository(repo =>
                {
                    repo.Connection = "mongodb://root:m0ng0_r00t@localhost:27017";
                    repo.DatabaseName = "OrderProcess";

                    repo.CollectionName = "orders";
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