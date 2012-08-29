namespace Ring.io
{
    using System.Net;

    public interface ITransportFactory
    {       
        ITransport Create(IPEndPoint endPoint);
    }
}