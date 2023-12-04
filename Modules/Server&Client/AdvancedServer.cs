using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Walhalla
{
    public class AdvancedServer : TcpServer
    {
        public Dictionary<IPEndPoint, AdvancedClient> Endpoints;
        public UdpHandler globalUdp;
        public int UdpPort;

        /// <summary> Simple server that handles tcp and udp </summary>
        public AdvancedServer(int tcpPort = 5000, int udpPort = 5001, bool accept = true) : base(tcpPort, false)
        {
            Endpoints = new Dictionary<IPEndPoint, AdvancedClient>();
            UdpPort = udpPort;

            globalUdp = new UdpHandler(udpPort, _receiveUdp);

            // Async client accept
            if (accept) auth();
        }

        /// <summary> Creates new tcp/udp client </summary>
        protected override ClientBase newClient(ref TcpClient tcp, uint uid)
        {
            AdvancedClient client = new AdvancedClient(ref tcp, uid, ref Clients, UdpPort);

            if (client.endPoint != null)
                lock (Endpoints)
                {
                    if (Endpoints.ContainsKey(client.endPoint)) Endpoints[client.endPoint] = client;
                    else Endpoints.Add(client.endPoint, client);
                }

            return client;
        }

        private void _receiveUdp(byte key, BufferType type, byte[] bytes, IPEndPoint endpoint)
        {
            lock (Endpoints)
            {
                if (Endpoints.TryGetValue(endpoint, out AdvancedClient? client))
                    client.onReceive(key, type, bytes, false);
            }
        }
    }

    public class AdvancedClient : SimpleClient
    {
        public IPEndPoint? endPoint;
        public UdpHandler? udp;

        public AdvancedClient(ref TcpClient client, uint uid, ref Dictionary<uint, ClientBase> registry, int udpPort) : base(ref client, uid, ref registry)
        {
            endPoint = client.Client.RemoteEndPoint as IPEndPoint;

            if (endPoint != null)
            {
                udp = new UdpHandler(endPoint.Address.ToString(), udpPort, _receiveUdp); // Use the same port as the UDP listener and the same adress as tcp endpoint
            }
            else udp = null;
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

            base.onDisconnect();
        }

        private void _receiveUdp(byte key, BufferType type, byte[] buffer)
            => onReceive(key, type, buffer, false);
    }
}