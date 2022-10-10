namespace Model.Abstractions
{
    public interface IMessage
    {
        Guid CorrelationId { get; }

        Order Order { get; }
    }
}