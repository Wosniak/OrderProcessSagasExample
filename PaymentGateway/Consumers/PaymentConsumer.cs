using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;

namespace PaymentGateway.Consumers
{
    public class PaymentConsumer : IConsumer<IProcessPayment>
    {
        private readonly ILogger<PaymentConsumer> _logger;
        public PaymentConsumer(ILogger<PaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IProcessPayment> context)
        {
            _logger.LogInformation($"Processando o pagamento para o pedido {context.Message.CorrelationId}");
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<IPaymentProcessed>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.Paymented;

    }
}
