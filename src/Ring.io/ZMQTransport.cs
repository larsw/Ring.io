namespace Ring.io
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    using ServiceStack.Text;
    using ZMQ;

    using Ring.io.Messages;
 
    public interface ITransport 
    {
        void Open();
        void Close();
        void Send(string text, IPEndPoint endPoint);
        void RegisterResponseHandler(IResponseHandler handler);
        void RegisterRequestHandler(IRequestHandler handler);
        void Prepare(Message message);
    }

    public class ZMQTransport : ITransport, IDisposable
    {
        private Context context;
        private Socket socket;
        private readonly JsonSerializer<Message> serializer;
        private bool _disposed;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private List<IRequestHandler> requestHandlers { get; set; }
        private List<IResponseHandler> responseHandlers { get; set; }

        public ZMQTransport(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
            this.serializer = new JsonSerializer<Message>();
            this.requestHandlers = new List<IRequestHandler>();
            this.responseHandlers = new List<IResponseHandler>();
        }

        private IPEndPoint EndPoint { get; set; }

        public void Open()
        {
            if (_disposed) throw new ObjectDisposedException("Object is disposed.");

            // Initialize ZeroMQ socket that will receive heartbeats.
            this.context = new Context();
            this.socket = this.context.Socket(SocketType.REP);
            this.socket.Bind(string.Format("tcp://{0}:{1}", this.EndPoint.Address, this.EndPoint.Port));

            Task.Factory.StartNew(() =>
                                      {
                                          while (true)
                                          {
                                              var bytes = this.socket.Recv();
                                              var request = Encoding.UTF8.GetString(bytes);
                                              this.HandleMessage(request, this.socket);
                                          }
                                      }, _cancellationTokenSource.Token);
        }

        public void Close()
        {
            if (_disposed) throw new ObjectDisposedException("Object is disposed.");
            Dispose();
        }

        public void Send(string text, IPEndPoint endPoint)
        {
            if (_disposed) throw new ObjectDisposedException("Object is disposed.");



            Task.Factory.StartNew(() =>
                                      {
                                          using (var ctx = new Context())
                                          using (var requestSocket = ctx.Socket(SocketType.REQ))
                                          {
                                              string addressString = "tcp://" + endPoint;
                                              requestSocket.Connect(addressString);
                                              requestSocket.Send(text, Encoding.UTF8);
                                              var response = requestSocket.Recv(Encoding.UTF8);
                                              HandleMessage(response, requestSocket);
                                          }
                                      }, _cancellationTokenSource.Token);
        }

        public void RegisterResponseHandler(IResponseHandler handler)
        {
            responseHandlers.Add(handler);
        }

        public void RegisterRequestHandler(IRequestHandler handler)
        {
            requestHandlers.Add(handler);
        }

        private void HandleMessage(string message, Socket requestSocket)
        {
            if (message != string.Empty)
            {
                var request = this.serializer.DeserializeFromString(message);

                System.Diagnostics.Debug.WriteLine("{0} RECV {1}", this.EndPoint.Port, request.Id);

                Message response = null;
                if (request.CorrelationId == null)
                {
                    response = new Message
                                   {
                                       CorrelationId = request.Id, 
                                       Destination = request.Source
                                   };
                    foreach (var handler in this.requestHandlers)
                    {
                        handler.HandleRequest(request, response);
                    }
                }
                else
                {
                    foreach (var handler in this.responseHandlers)
                    {
                        handler.HandleResponse(request);
                    }
                }

                if (request.CorrelationId == null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "{0} SENT {1}",
                        this.EndPoint.Port,
                        response.Id));

                    var msg = this.serializer.SerializeToString(response);
                    requestSocket.Send(msg, Encoding.UTF8);
                }
            }
        }

        public void Prepare(Message message)
        {
            message.Source = EndPoint.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    socket.Dispose();
                    context.Dispose();
                }
                socket = null;
                context = null;
                _disposed = true;
            }
        }

        ~ZMQTransport()
        {
            Dispose(false);
        }
    }
}