using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;

namespace Shipment.Consumer
{
    public class ShipmentConsumer : IConsumer<IShipOrder>
    {
        private readonly ILogger<ShipmentConsumer> _logger;
        public ShipmentConsumer(ILogger<ShipmentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IShipOrder> context)
        {
            _logger.LogInformation($"Processando envio para o pedido {context.Message.CorrelationId}");
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<IOrderShipped>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.Shipped;

    }
}
