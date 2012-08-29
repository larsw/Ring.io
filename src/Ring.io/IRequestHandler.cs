namespace Ring.io
{
    using Ring.io.Messages;

    public interface IRequestHandler
    {
        void HandleRequest(Message request, Message response);
    }
}
