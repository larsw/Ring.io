namespace Ring.io
{
    using System.Net;
    using Messages;

    public interface IMessageBus
    {
        void Send(Message message, IPEndPoint endPoint);
        Message CreateMessage(object part);
    }
}