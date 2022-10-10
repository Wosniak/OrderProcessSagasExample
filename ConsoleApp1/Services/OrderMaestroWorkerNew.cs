using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Model.Abstractions;
using Model.Abstractions.Commands;
using Model.Abstractions.Events;
using OrderMaestro.Events;
using OrderMaestro.State;

namespace OrderMaestro.Services
{
    internal class OrderMaestoStateMachineNew : MassTransitStateMachine<OrderProcessState>
    {
        private readonly ILogger<OrderMaestoStateMachine> _logger;
        private readonly IConfiguration _configuration;
        public OrderMaestoStateMachineNew(ILogger<OrderMaestoStateMachine> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            InstanceState(instance => instance.State);
            ConfigurCorrelateIds();

            Initially(
                When(OrderSubmitted)
                    .Then(c => UpdateSagaState(c.Saga, c.Message.Order))
                    .Then(c => Console.WriteLine($"Pedido {c.Message.CorrelationId} recebido"))
                    .IfElse(x => x.Message.Order.Items.Any(i => i.Product.Type == ProductType.Fisical), cmd => cmd.ThenAsync(c => SendCommand<IReserveStock>("Queues:Warehouse", c)),
                        cmdElse => cmdElse.ThenAsync(c => SendCommand<IProcessPayment>("Queues:Payment", c)).TransitionTo(OrderAccepted)),
                    
                When(StockReserved)
                    .Then(c => UpdateSagaState(c.Saga, c.Message.Order))
                        .Then(c => Console.WriteLine($"Solicitação de itens para o pedido {c.Message.CorrelationId} recebido"))
                        .ThenAsync(c => SendCommand<IProcessPayment>("Queues:Payment", c)).TransitionTo(OrderAccepted)
                );


            During(OrderAccepted,
                When(PaymentProcessed)
                    .Then(c => UpdateSagaState(c.Saga, c.Message.Order))
                    .Then(c => Console.WriteLine($"Processamento do pagamento para pedido {c.Message.CorrelationId} recebido"))
                    .IfElse(x => x.Message.Order.Items.Any(i => i.Product.HasRoyaltiesFees), 
                        cmd => cmd.Then(async c => await SendCommand<IProcessRoyalties>("Queues:RoyaltiesFee", c)),
                        elseCmd =>  elseCmd.Then(async c => {
                            await Task.Delay(2000);
                            c.Message.Order.Status = Status.RoyaltiesProcessed;
                            await PublishMessage<IRoyaltiesProcessed>(c);
                        })),
                When(RoyaltiesProcessed)
                    .Then(c => UpdateSagaState(c.Saga, c.Message.Order))
                    .Then(c => Console.WriteLine($"Processamento dos Direitos Autorais para pedido {c.Message.CorrelationId} recebido"))
                    .IfElse(x => x.Message.Order.Items.Any(i => i.Product.CommisionPercentage > 0),
                        cmd => cmd.Then(async c => await SendCommand<IProcessCommission>("Queues:Payroll", c)),
                        elseCmd => elseCmd.Then(async c => {
                            await Task.Delay(2000);
                            c.Message.Order.Status = Status.CommisionProcessed;
                            await PublishMessage<ICommissionProcessed>(c);
                        } )),
                When(CommisionProcessed)
                    .Then(c => UpdateSagaState(c.Saga, c.Message.Order))
                    .Then(c => Console.WriteLine($"Processamento da Comissão para pedido {c.Message.CorrelationId} recebido"))
                    .If(x => x.Message.Order.Items.Any(i => i.Product.Type == ProductType.Fisical), cmd => cmd.ThenAsync(c => SendCommand<IShipOrder>("Queues:Shipment", c)))
                    .If(x => x.Message.Order.Items.Any(i => i.Product.Type == ProductType.Subscription), cmd => cmd.ThenAsync(c => SendCommand<IManageSubscription>("Queues:Subscription", c)))
                    .TransitionTo(OrderReady)
                );

            During(OrderReady,
                When(SubscriptionActivated)
                    .Then(c =>
                    {
                        this.UpdateSagaState(c.Saga, c.Message.Order);
                        c.Saga.Order.Status = Status.Processed;
                    })
                    .Then(c => Console.WriteLine($"Assinatura do pedido {c.Message.CorrelationId} processada."))
                    .Publish(c => new OrderProcessed(c.Message.CorrelationId, c.Message.Order))
                    .Finalize(),
                When(OrderShipped)
                    .Then(c =>
                    {
                        this.UpdateSagaState(c.Saga, c.Message.Order);
                        c.Saga.Order.Status = Status.Processed;
                    })
                    .Then(c => Console.WriteLine($"Pedido {c.Message.CorrelationId} enviado."))
                    .Publish(c => new OrderProcessed(c.Message.CorrelationId, c.Message.Order))
                    .Finalize()
            );            


            SetCompletedWhenFinalized();
        }
        private async Task SendCommand<TCommand>(string endpointKey, BehaviorContext<OrderProcessState, IMessage> context)
            where TCommand : class, IMessage
        {
            var sendEndpoint = await context.GetSendEndpoint(new Uri($"rabbitmq://localhost/{_configuration.GetSection(endpointKey).Value}"));
            await sendEndpoint.Send<TCommand>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });
        }

        private async Task PublishMessage<TMessage>(BehaviorContext<OrderProcessState, IMessage> context)
            where TMessage : class, IMessage
        {   await context.Publish<TMessage>(new
            {
                CorrelationId = context.Message.CorrelationId,
                Order = context.Message.Order
            });
            
        }


        private void UpdateSagaState(OrderProcessState state, Order order)
        {
            var currentDate = DateTime.Now;
            state.Created = currentDate;
            state.Updated = currentDate;
            state.Order = order;
        }
        private void ConfigurCorrelateIds()
        {
            Event(() => OrderSubmitted, action => action.CorrelateById(ctx => ctx.Message.CorrelationId).SelectId(ctx => ctx.Message.CorrelationId));
            Event(() => StockReserved, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
            Event(() => PaymentProcessed, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
            Event(() => OrderShipped, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
            Event(() => CommisionProcessed, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
            Event(() => RoyaltiesProcessed, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
            Event(() => SubscriptionActivated, action => action.CorrelateById(ctx => ctx.Message.CorrelationId));
        }
        
        public Event<IOrderSubmitted>? OrderSubmitted { get; private set; }
        public Event<IOrderShipped>? OrderShipped { get; set; }
        public Event<IPaymentProcessed>? PaymentProcessed { get; private set; }
        public Event<IStockReserved>? StockReserved { get; private set; }

        public Event<ISubscriptionActivated>? SubscriptionActivated { get; private set; }
        public Event<ICommissionProcessed>? CommisionProcessed { get; private set; }
        public Event<IRoyaltiesProcessed>? RoyaltiesProcessed { get; private set; }

        public MassTransit.State? Processing { get; private set; }
        public MassTransit.State? OrderPayed { get; private set; }

        public MassTransit.State OrderAccepted { get; private set; }
        public MassTransit.State OrderReady { get; private set; }
        public MassTransit.State LegalObligations { get; private set; }
    }
}
