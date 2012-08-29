namespace Ring.io
{
    public class MessageBusFactory : IMessageBusFactory
    {
        private readonly ITransportFactory _transportFactory;

        public MessageBusFactory()
            :this(new TransportFactory())
        {
            
        }

        public MessageBusFactory(ITransportFactory transportFactory)
        {
            _transportFactory = transportFactory;
        }

        public IMessageBus Create(Node node)
        {
            return new MessageBus(node, _transportFactory.Create(node.Endpoint));
        }
    }
}