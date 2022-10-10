using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;

namespace Payroll.Consumers
{
    internal class CommissionPaymentConsumer : IConsumer<IProcessCommission>
    {
        private readonly ILogger<CommissionPaymentConsumer> _logger;
        public CommissionPaymentConsumer(ILogger<CommissionPaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IProcessCommission> context)
        {
            var products = context.Message.Order.Items.Where(item => item.Product.CommisionPercentage > 0).Select(i => i.Product);

            foreach (var product in products)
            {
                _logger.LogInformation($"Pagamento de comissão de R$ {product.Price * (product.CommisionPercentage / 100)} para o pedido {context.Message.CorrelationId} gerado");
            }

            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<ICommissionProcessed>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.CommisionProcessed;


    }
}
