using System.Net.Sockets;
using System.Net;

namespace Walhalla
{
    public class SimpleServer<Conn> where Conn : SimpleServer<Conn>.Connection
    {
        protected Dictionary<int, NetworkStream> Streams;
        protected TcpListener TcpListener;
        protected int LastUID;

        public SimpleServer(int port = 5000)
        {
            Streams = new Dictionary<int, NetworkStream>();
            LastUID = 0;

            TcpListener = new TcpListener(IPAddress.Loopback, port);
            TcpListener.Start(10);

            // Async client accept
            acceptTcp();

            // Synchronous alternative.
            // var acceptedSocket = listener.AcceptSocket();
        }

        public virtual async void acceptTcp()
        {
            using var acceptedSocket = await TcpListener.AcceptSocketAsync();
            using NetworkStream stream = acceptedSocket.GetStream();

            lock (Streams)
            {
                $"Connected [{LastUID}]".Log();
                Streams.Add(LastUID++, stream);
            }
        }

        public class Connection
        {
            public bool verified;
            public uint uid;
        }
    }
}