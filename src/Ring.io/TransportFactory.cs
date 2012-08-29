namespace Ring.io
{
    using System.Net;

    public class TransportFactory : ITransportFactory
    {
        public ITransport Create(IPEndPoint endPoint)
        {
            return new ZMQTransport(endPoint);
        }
    }
}