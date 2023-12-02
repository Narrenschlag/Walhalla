using System.Threading.Tasks;
using System.Net.Sockets;

namespace Walhalla
{
    public class UdpHandler : HandlerBase
    {
        public UdpClient client;

        /// <summary> Creates handle on server side </summary>
        public UdpHandler(int port, Packet onReceive) : base(port, onReceive)
        {
            try
            {
                client = new UdpClient(port);
                $"udp: {port}".Log();
            }
            catch (Exception ex)
            {
                throw new Exception($"Was not able to establish a udp connection:\n{ex.Message}");
            }

            _ = _listen();
        }

        /// <summary> Creates handle on client side </summary>
        public UdpHandler(string host, int port, Packet onReceive) : base(port, onReceive)
        {
            client = new UdpClient();
            client.Connect(host, port);

            _ = _listen();
        }

        /// <summary> Closes local network elements </summary>
        public override void Close()
        {
            if (client != null) client.Close();

            base.Close();
        }

        #region Send Data
        /// <summary> Sends data through connection </summary>
        public override void send<T>(byte key, T value)
        {
            base.send(key, value);

            client.Send(value.encodeBytes(key));
        }

        /// <summary> Sends data through connection </summary>
        public override void send(byte key, BufferType type, byte[] bytes)
        {
            base.send(key, type, bytes);

            if (bytes == null) bytes = new byte[0];
            client.Send(bytes.encodeBytes(type, key));
        }
        #endregion

        #region Receive Data
        protected override async Task _receive()
        {
            // Read length
            UdpReceiveResult result = await client.ReceiveAsync();
            byte[] buffer = result.Buffer;

            byte[] bytes = Bufferf.decodeBytes(buffer, out int length, out BufferType type, out byte key);
            if (onReceive != null && type != BufferType.None) onReceive(type, key, bytes);
        }
        #endregion
    }
}