namespace Ring.io
{
    using System;
    using System.Net;
    using ServiceStack.Text;

    using Ring.io.Messages;

    public class MessageBus : IRequestHandler, IResponseHandler, IMessageBus 
    {
        private Node node;
        private readonly ITransport transport;
        private readonly JsonSerializer<Message> serializer;

        public MessageBus(Node node, ITransport transport)
        {
            this.node = node;
            this.transport = transport;
            serializer = new JsonSerializer<Message>();

            this.transport.RegisterRequestHandler(this);
            this.transport.RegisterResponseHandler(this);
        }

        public void HandleRequest(Message request, Message response)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format(
            //    "{0}\t{1} received {2}",
            //    DateTime.Now,
            //    this.node.Entry.Address,
            //    sourceEndPoint.Port));

            // TODO: Here we handle the messages that get received by the node.
            // TODO: Call methods on the Node class to merge hash rings and do failure detection.
            // TODO: Liskov substitution principle violation that I'm not happy about.

            if (response != null)
            {
                response.Source = this.node.Entry.Address;

                var heartbeat = new HeartBeat();
                heartbeat.Nodes = node.Nodes;

                response.Parts.Add(heartbeat.GetType().Name.ToLowerInvariant(), JsonSerializer.SerializeToString(heartbeat));

                string[] sourceAddress = request.Source.Split(':');
                var sourceEndPoint = new IPEndPoint(IPAddress.Parse(sourceAddress[0]), int.Parse(sourceAddress[1]));
            }
        }

        public void HandleResponse(Message response)
        {
            // TODO: Here we handle the messages that get received by the node.
            // TODO: Call methods on the Node class to merge hash rings and do failure detection.
            // TODO: Liskov substitution principle violation that I'm not happy about.

            //System.Diagnostics.Debug.WriteLine(string.Format(
            //    "{0}\t{1} received {2}",
            //    DateTime.Now,
            //    this.node.Entry.Address,
            //    sourceEndPoint.Port));
        }

        public void Send(Message message, IPEndPoint endPoint)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(
                "{0} SENT {1}",
                this.node.Entry.Address,
                message.Id));

            string msg = this.serializer.SerializeToString(message);
            transport.Prepare(message);
            this.transport.Send(msg, endPoint);
        }

        public Message CreateMessage(object part)
        {
            var message = new Message();
            message.Parts.Add(part.GetType().Name.ToLowerInvariant(), JsonSerializer.SerializeToString(part));
            return message;
        }
    }
}