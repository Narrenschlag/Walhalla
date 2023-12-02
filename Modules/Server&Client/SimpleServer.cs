using System.Net.Sockets;
using System.Net;
using Walhalla;

namespace Walhalla
{
    public class SimpleServer
    {
        public Dictionary<uint, ClientBase> Clients;
        public int Port;

        protected TcpListener TcpListener;
        protected uint LastUID;

        /// <summary> Simple server that handles tcp only </summary>
        public SimpleServer(int port = 5000)
        {
            Clients = new Dictionary<uint, ClientBase>();
            LastUID = 0;
            Port = port;

            TcpListener = new TcpListener(IPAddress.Any, port);
            TcpListener.Start(10);

            // Async client accept
            accept();
        }

        /// <summary>
        /// Handles all incomming connections and assigns them to an id<br/>
        /// Then it starts listening to them
        /// </summary>
        protected virtual async void accept()
        {
            TcpClient tcp = await TcpListener.AcceptTcpClientAsync();  //if a connection exists, the server will accept it

            lock (this)
            {
                newClient(ref tcp, LastUID++);
            }

            accept();  // welcome other clients
        }

        /// <summary> Creates new tcp-only client </summary>
        protected virtual void newClient(ref TcpClient tcp, uint uid)
        {
            new SimpleClient(ref tcp, uid, ref Clients);
        }
    }

    public class SimpleClient : ClientBase
    {
        public TcpHandler tcp;

        public SimpleClient(ref TcpClient client, uint uid, ref Dictionary<uint, ClientBase> registry) : base(uid, ref registry)
        {
            tcp = new TcpHandler(ref client, uid, receiveTcp, onDisconnect);
        }

        private void receiveTcp(BufferType type, byte key, byte[]? bytes)
            => onReceive(type, key, bytes, true);

        public override void send<T>(byte key, T value, bool tcp)
        {
            if (tcp) this.tcp.send(key, value);
        }

        public override void send(BufferType type, byte key, byte[]? bytes, bool tcp)
        {
            if (tcp) this.tcp.send(type, key, bytes);
        }
    }
}

public class ClientBase
{
    protected Dictionary<uint, ClientBase> Registry;
    public uint UID;

    public ClientBase(uint uid, ref Dictionary<uint, ClientBase> registry)
    {
        $"+++ Connected [{uid}]".Log();

        Registry = registry;
        UID = uid;

        lock (Registry)
        {
            Registry.Add(UID, this);
        }
    }

    public virtual void send<T>(byte key, T value, bool tcp)
    {

    }

    public virtual void send(BufferType type, byte key, byte[]? bytes, bool tcp)
    {

    }

    /// <summary> Handles incomming traffic </summary>
    public virtual void onReceive(BufferType type, byte key, byte[]? bytes, bool tcp)
    {
        $"Received: [{type}] sizeof({(bytes == null ? "0" : bytes.Length)}) as {key}".Log();
    }

    public virtual void onDisconnect()
    {
        $"--- Disconnected [{UID}]".Log();

        lock (Registry)
        {
            Registry.Remove(UID);
        }
    }
}