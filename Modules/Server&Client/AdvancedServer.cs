using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Walhalla
{
    public class AdvancedServer : TcpServer
    {
        public Dictionary<IPEndPoint, AdvancedClient> Endpoints;
        public Dictionary<IPAddress, AdvancedClient> Queue;
        public UdpHandler globalUdp;
        public int UdpPort;

        /// <summary> Simple server that handles tcp and udp </summary>
        public AdvancedServer(int tcpPort = 5000, int udpPort = 5001, bool accept = true) : base(tcpPort, false)
        {
            Endpoints = new Dictionary<IPEndPoint, AdvancedClient>();
            Queue = new Dictionary<IPAddress, AdvancedClient>();
            UdpPort = udpPort;

            globalUdp = new UdpHandler(udpPort, _receiveUdp);

            // Async client accept
            if (accept) auth();
        }

        /// <summary> Creates new tcp/udp client </summary>
        protected override ClientBase newClient(ref TcpClient tcp, uint uid)
        {
            AdvancedClient client = new AdvancedClient(ref tcp, uid, ref Clients, this);

            if (client.endPoint != null)
                lock (Queue)
                {
                    IPAddress addr = client.endPoint.Address;
                    $"Register: {addr}".Log();

                    if (Queue.ContainsKey(addr)) Queue[addr] = client;
                    else Queue.Add(addr, client);
                }

            return client;
        }

        private void _receiveUdp(byte key, BufferType type, byte[] bytes, IPEndPoint endpoint)
        {
            lock (this)
            {
                // Move queued element to endpoint registry
                if (!Endpoints.TryGetValue(endpoint, out AdvancedClient? client))
                {
                    if (Queue.TryGetValue(endpoint.Address, out client))
                    {
                        Endpoints.Add(endpoint, client);
                        Queue.Remove(endpoint.Address);

                        client.connect(endpoint);
                    }
                }

                if (client != null)
                    client.onReceive(key, type, bytes, false);
            }
        }
    }

    public class AdvancedClient : SimpleClient
    {
        public AdvancedServer server;
        public IPEndPoint? endPoint;
        public UdpHandler? udp;

        public AdvancedClient(ref TcpClient client, uint uid, ref Dictionary<uint, ClientBase> registry, AdvancedServer server) : base(ref client, uid, ref registry)
        {
            endPoint = client.Client.RemoteEndPoint as IPEndPoint;
            this.server = server;
            udp = null;
        }

        public void connect(IPEndPoint finalSource)
        {
            if (endPoint == null) return;
            endPoint = finalSource;

            udp = new UdpHandler(endPoint.Address.ToString(), server.UdpPort, _receiveUdp); // Use the same port as the UDP listener and the same adress as tcp endpoint
        }

        public override bool Connected => base.Connected && ConnectedUdp;
        public bool ConnectedUdp => udp != null && udp.Connected;

        public override void send(byte key, BufferType type, byte[] bytes, bool tcp)
        {
            base.send(key, type, bytes, tcp);

            if (!tcp && ConnectedUdp && udp != null)
            {
                udp.send(key, type, bytes);
            }
        }

        public override void send<T>(byte key, T value, bool tcp)
        {
            base.send(key, value, tcp);

            if (!tcp && ConnectedUdp && udp != null)
            {
                udp.send(key, value);
            }
        }

        public override void onDisconnect()
        {
            if (udp != null) udp.Close();

            lock (server.Endpoints)
            {
                if (endPoint != null && server.Endpoints.ContainsKey(endPoint))
                    server.Endpoints.Remove(endPoint);
            }

            base.onDisconnect();
        }

        private void _receiveUdp(byte key, BufferType type, byte[] buffer)
            => onReceive(key, type, buffer, false);
    }
}