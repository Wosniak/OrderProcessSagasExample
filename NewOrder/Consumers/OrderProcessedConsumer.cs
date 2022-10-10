using MassTransit;
using Microsoft.Extensions.Logging;
using Model.Abstractions.Events;

namespace NewOrder.Consumers
{
    public class OrderProcessedConsumer : IConsumer<IOrderProcessed>
    {
        private readonly ILogger<OrderProcessedConsumer> _logger;

        public OrderProcessedConsumer(ILogger<OrderProcessedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IOrderProcessed> context)
        {
            _logger.LogInformation($"Pedido {context.Message.CorrelationId} Processado");
        }
    }
}
