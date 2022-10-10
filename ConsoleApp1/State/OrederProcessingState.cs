using MassTransit;
using Model;

namespace OrderMaestro.State
{
    internal class OrderProcessState : SagaStateMachineInstance, ISagaVersion
    {

        public OrderProcessState(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Order Order { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public Guid CorrelationId { get; set; }

        public string State { get; set; }
        public int Version { get; set; }
    }
}
