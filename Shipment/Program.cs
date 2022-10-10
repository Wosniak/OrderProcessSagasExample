// See https://aka.ms/new-console-template for more information
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shipment.Consumer;

var configBuilder = new ConfigurationBuilder();
ConfigSetup(configBuilder);

var config = configBuilder.Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<ShipmentConsumer>();
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
                    cfg.ConfigureConsumer<ShipmentConsumer>(context);
                });
            });
            bus.AddConsumer<ShipmentConsumer>();
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