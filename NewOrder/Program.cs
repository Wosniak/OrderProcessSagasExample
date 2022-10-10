using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model.Abstractions.Events;
using NewOrder.Consumers;
using NewOrder.Generator;
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
                    cfg.ConfigureConsumer<OrderProcessedConsumer>(context);
                });

            });
            bus.AddConsumer<OrderProcessedConsumer>();

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
await host.StartAsync();



var continueRunning = true;

Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, args) =>
{
    args.Cancel = true;
    continueRunning = false;
});

using IServiceScope serviceScope = host.Services.CreateScope();

IServiceProvider provider = serviceScope.ServiceProvider;



var bus = provider.GetService<IBus>();


Console.WriteLine("Pressione Enter para enviar pedido");
while (continueRunning)
{
    Console.ReadKey();
    var order = OrderFactory.CreateOrder();

    Console.WriteLine($"Enviando Pedido...");

    if (bus != null)
    {
        await bus.Publish<IOrderSubmitted>(new { CorrelationId = Guid.NewGuid(), Order = order });
    }
}




static void ConfigSetup(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
}

