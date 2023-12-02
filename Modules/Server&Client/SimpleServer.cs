using System.Net.Sockets;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Numerics;
using System.Dynamic;

namespace Walhalla
{
    public class SimpleServer
    {
        public Dictionary<uint, SimpleClient> Clients;
        protected TcpListener TcpListener;
        protected uint LastUID;

        public SimpleServer(int port = 5000)
        {
            Clients = new Dictionary<uint, SimpleClient>();
            LastUID = 0;

            TcpListener = new TcpListener(IPAddress.Loopback, port);
            TcpListener.Start(10);

            // Async client accept
            welcome();
        }

        /// <summary>
        /// Handles all incomming connections and assigns them to an id
        /// Then it starts listening to them
        /// </summary>
        protected virtual async void welcome()
        {
            TcpClient tcp = await TcpListener.AcceptTcpClientAsync();  //if a connection exists, the server will accept it

            lock (this)
            {
                SimpleClient client = new SimpleClient(ref tcp, LastUID++, this);
            }

            welcome();  // welcome other clients
        }
    }

    public class SimpleClient
    {
        public TcpHandler tcpHandler;

        protected SimpleServer source;
        public uint UID;

        public SimpleClient(ref TcpClient client, uint uid, SimpleServer source)
        {
            $"+++ Connected [{uid}]".Log();
            this.source = source;
            UID = uid;

            lock (source.Clients)
            {
                source.Clients.Add(uid, this);
            }

            tcpHandler = new TcpHandler(ref client, uid, receiveTcp, onDisconnect);
        }

        public virtual void send<T>(byte key, T value, bool tcp)
        {
            if (tcp) tcpHandler.send(key, value);
        }

        public virtual void send(BufferType type, byte key, byte[]? bytes, bool tcp)
        {
            if (tcp) tcpHandler.send(type, key, bytes);
        }

        private void receiveTcp(BufferType type, byte key, byte[]? bytes)
            => onReceive(type, key, bytes, true);

        public virtual void onReceive(BufferType type, byte key, byte[]? bytes, bool tcp)
        {

        }

        // TODO: Fix instant disconnect
        public virtual void onDisconnect()
        {
            $"--- Disconnected [{UID}]".Log();

            lock (source.Clients)
            {
                source.Clients.Remove(UID);
            }
        }
    }
}