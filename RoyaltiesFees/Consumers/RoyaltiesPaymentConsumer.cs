using MassTransit;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyaltiesFees.Consumers
{
    internal class RoyaltiesPaymentConsumer : IConsumer<IProcessRoyalties>
    {
        private readonly ILogger<RoyaltiesPaymentConsumer> _logger;
        public RoyaltiesPaymentConsumer(ILogger<RoyaltiesPaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IProcessRoyalties> context)
        {
            decimal RoyaltiesValue = 5.5m;

            var products = context.Message.Order.Items.Where(item => item.Product.HasRoyaltiesFees).Select(i => i.Product);

            foreach (var product in products)
            {
                _logger.LogInformation($"Pagamento de direitos autorais de R$ {product.Price * (RoyaltiesValue/100)} para o pedido {context.Message.CorrelationId} gerado");
            }

            
            await Task.Delay(2000);
            UpdateOrderState(context.Message.Order);
            await context.Publish<IRoyaltiesProcessed>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });

        }

        private void UpdateOrderState(Order order) =>
            order.Status = Status.RoyaltiesProcessed;


    }
}
