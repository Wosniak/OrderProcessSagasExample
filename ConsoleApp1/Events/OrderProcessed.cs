using Model;
using Model.Abstractions.Events;

namespace OrderMaestro.Events
{
    internal class OrderProcessed : IOrderProcessed
    {
        public OrderProcessed(Guid correlationId, Order order)
        {
            this.CorrelationId = correlationId;
            this.Order = order;
        }
        public Guid CorrelationId { get; }

        public Order Order { get; }
    }
}
