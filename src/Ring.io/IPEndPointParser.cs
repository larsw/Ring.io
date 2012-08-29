namespace Ring.io
{
    using System;
    using System.Net;

    public class IPEndPointParser
    {
        public static IPEndPoint Parse(String endPoint)
        {
            string[] address = endPoint.Split(':');
            return new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1]));
        }
    }
}
