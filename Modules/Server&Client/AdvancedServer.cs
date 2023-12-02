using System.Net.Sockets;

namespace Walhalla
{
    public class AdvancedServer : TcpServer
    {
        public int UdpPort;

        /// <summary> Simple server that handles tcp and udp </summary>
        public AdvancedServer(int tcpPort = 5000, int udpPort = 5001, bool accept = true) : base(tcpPort, false)
        {
            UdpPort = udpPort;

            // Async client accept
            if (accept) auth();
        }

        /// <summary> Creates new tcp/udp client </summary>
        protected override ClientBase newClient(ref TcpClient tcp, uint uid)
        {
            return new AdvancedClient(ref tcp, uid, ref Clients, UdpPort);
        }
    }

    public class AdvancedClient : SimpleClient
    {
        public UdpHandler udp;

        public AdvancedClient(ref TcpClient client, uint uid, ref Dictionary<uint, ClientBase> registry, int udpPort) : base(ref client, uid, ref registry)
        {
            udp = new UdpHandler(udpPort, receiveUdp);

            $"#######\nConnected: {Connected}\ntcp-connection: {tcp.Connected}\nudp-connection: {udp.Connected}\n#######".Log();
        }

        public override bool Connected => base.Connected && ConnectedUdp;
        public bool ConnectedUdp => udp != null && udp.Connected;

        public override void send<T>(byte key, T value, bool tcp)
        {
            base.send(key, value, tcp);

            if (!tcp && ConnectedUdp) udp.send(key, value);
        }

        public override void send(byte key, BufferType type, byte[] bytes, bool tcp)
        {
            base.send(key, type, bytes, tcp);

            if (!tcp && ConnectedUdp) udp.send(key, type, bytes);
        }

        private void receiveUdp(BufferType type, byte key, byte[] bytes)
            => onReceive(type, key, bytes, false);

        public override void onDisconnect()
        {
            udp.Close();

            base.onDisconnect();
        }
    }
}