namespace Ring.io
{
    public interface IMessageBusFactory
    {
        IMessageBus Create(Node node);
    }
}