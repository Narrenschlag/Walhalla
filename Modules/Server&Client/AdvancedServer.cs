using System.Net.Sockets;

namespace Walhalla
{
    public class AdvancedServer : SimpleServer
    {
        public int UdpPort;

        /// <summary> Simple server that handles tcp and udp </summary>
        public AdvancedServer(int tcpPort = 5000, int udpPort = 5001) : base(tcpPort)
        {
            UdpPort = udpPort;
        }

        /// <summary> Creates new tcp/udp client </summary>
        protected override void newClient(ref TcpClient tcp, uint uid)
        {
            new AdvancedClient(ref tcp, uid, ref Clients, UdpPort);
        }
    }

    public class AdvancedClient : SimpleClient
    {
        public UdpHandler udp;

        public AdvancedClient(ref TcpClient client, uint uid, ref Dictionary<uint, ClientBase> registry, int udpPort) : base(ref client, uid, ref registry)
        {
            udp = new UdpHandler(udpPort, receiveUdp);
        }

        public override void send<T>(byte key, T value, bool tcp)
        {
            base.send(key, value, tcp);

            if (!tcp) udp.send(key, value);
        }

        public override void send(byte key, BufferType type, byte[]? bytes, bool tcp)
        {
            base.send(key, type, bytes, tcp);

            if (!tcp) udp.send(key, type, bytes);
        }

        private void receiveUdp(BufferType type, byte key, byte[]? bytes)
            => onReceive(type, key, bytes, false);

        public override void onDisconnect()
        {
            udp.close();

            base.onDisconnect();
        }
    }
}