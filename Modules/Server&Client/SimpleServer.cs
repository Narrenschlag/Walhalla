using System.Net.Sockets;
using System.Net;
using System.Text;

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
        public NetworkStream tcpStream;
        public TcpClient tcpClient;

        protected SimpleServer source;
        public uint UID;

        public SimpleClient(ref TcpClient client, uint uid, SimpleServer source)
        {
            tcpClient = client;

            tcpStream = client.GetStream();
            this.source = source;
            UID = uid;

            welcome();
            listen();
        }

        public virtual void welcome()
        {
            $"+++ Connected [{UID}]".Log();

            source.Clients.Add(UID, this);
        }

        /// <summary>
        /// Handles all incomming messages from a client and it's network stream
        /// </summary>
        public virtual async void listen()
        {
            // Set timeout to 5 minutes
            tcpClient.ReceiveTimeout = 6000 * 5;
            $"now listening to [{UID}]".Log();

            // For some reason weird stuff happens if this is not written to stream
            tcpStream.Write(BitConverter.GetBytes(UID));  // Welcome to server
            tcpStream.Flush();

            while (tcpClient.Connected)  //while the client is connected, we look for incoming messages
            {
                byte[] buffer = new byte[1024];

                try
                {
                    await tcpStream.ReadAsync(buffer, 0, buffer.Length);    //the same networkstream reads the message sent by the client
                }
                catch
                {
                    break;
                }

                read(buffer, true);    // Now we read the data
            }

            onDisconnect();
        }

        public virtual void onDisconnect()
        {
            $"--- Disconnected [{UID}]".Log();

            // Close client and stream
            tcpClient.Close();
            tcpStream.Close();

            lock (source.Clients)
            {
                source.Clients.Remove(UID);
            }
        }

        public virtual void read(byte[] buffer, bool tcp)
        {
            $"[{UID}]: package({buffer.Length})".Log();
        }
    }
}