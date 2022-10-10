using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;

namespace Warehouse.Consumers
{
    public class ReserveStockConsumer : IConsumer<IReserveStock>
    {
        private readonly ILogger<ReserveStockConsumer> _logger;
        public ReserveStockConsumer(ILogger<ReserveStockConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IReserveStock> context)
        {
            _logger.LogInformation($"Separação de produto para o pedido {context.Message.CorrelationId} recebido");
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<IStockReserved>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.StockReserved;


    }
}
