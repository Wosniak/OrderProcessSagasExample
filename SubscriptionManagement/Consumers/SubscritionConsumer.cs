using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;

namespace SubscriptionManagement.Consumer
{
    public class SubscritionConsumer : IConsumer<IManageSubscription>
    {
        private readonly ILogger<SubscritionConsumer> _logger;
        public SubscritionConsumer(ILogger<SubscritionConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IManageSubscription> context)
        {
            _logger.LogInformation($"Assinaturado pedido {context.Message.CorrelationId} processada");
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<ISubscriptionActivated>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.Processed;
    }
}
