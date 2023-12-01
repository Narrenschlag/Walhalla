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
        public virtual void listen()
        {
            // Set timeout to 5 minutes
            tcpClient.ReceiveTimeout = 6000 * 5;
            $"now listening to [{UID}]".Log();

            // For some reason weird stuff happens if this is not written to stream
            SendTcp(0, UID, true);  // Welcome to server

            while (tcpClient.Connected)  //while the client is connected, we look for incoming messages
            {
                try
                {
                    receiveTcp();    //the same networkstream reads the message sent by the client
                }
                catch
                {
                    break;
                }
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

        #region Send
        public void SendTcp<T>(short key, T? value, bool flush = true)
        {
            $">>> [{typeof(T)}]".Log();

            tcpStream.Write(prepareBytes(value, key));
            if (flush) Flush(true);
        }

        public void SendTcp(TypeId typeId, short key, byte[] bytes, bool flush = true)
        {
            tcpStream.Write(prepareBytes(bytes, typeId, key));
            if (flush) Flush(true);
        }

        public virtual void Flush(bool tcp)
        {
            if (tcp) tcpStream.Flush();
        }

        public static byte[] prepareBytes<T>(T value, short key) => prepareBytes(toBytes(value, out TypeId typeId), typeId, key);
        public static byte[] prepareBytes(byte[] bytes, TypeId typeId, short key)
        {
            byte[] buffer = new byte[bytes.Length + 8];

            // Writes length of array into size array
            using (MemoryStream mem = new MemoryStream(buffer))
            {
                using (BinaryWriter BW = new BinaryWriter(mem))
                {
                    BW.Write(bytes.Length);
                    BW.Write((ushort)typeId);
                    BW.Write(key);
                }
            }

            Array.Copy(bytes, 0, buffer, 6, bytes.Length);
            return buffer;
        }
        #endregion

        #region Receive
        public async void receiveTcp()
        {
            // Define buffer for strorage
            byte[] buffer = new byte[4];

            // Read length
            await tcpStream.ReadAsync(buffer, 0, 4);
            int length = readBytes(buffer, true);

            // Read type bytes
            resize(2);
            await tcpStream.ReadAsync(buffer, 0, 2);
            short type = (short)readBytes(buffer, false);

            // Read key type
            resize(2);
            await tcpStream.ReadAsync(buffer, 0, 2);
            short key = (short)readBytes(buffer, false);

            // Resize array to length
            resize(length);

            // Read byte array of the given length
            await tcpStream.ReadAsync(buffer, 0, length);
            read((TypeId)type, key, buffer, true);

            // Restart receiving
            receiveTcp();

            void resize(int newSize)
            {
                Array.Clear(buffer);
                Array.Resize(ref buffer, newSize);
            }
        }

        public virtual void read(TypeId typeId, short key, byte[] buffer, bool tcp)
        {
            $"[{UID}]: package({typeId})".Log();
        }

        private static int readBytes(byte[] bytes, bool is32)
        {
            // Read length of buffer
            using (MemoryStream mem = new MemoryStream(bytes))
            {
                using (BinaryReader BR = new BinaryReader(mem))
                {
                    if (is32) return BR.ReadInt32();
                    else return BR.ReadInt16();
                }
            }
        }
        #endregion

        #region Bytes and Ids
        public static byte[] toBytes<T>(T? value, out TypeId typeId)
        {
            if (value == null)
            {
                typeId = TypeId.None;
                return @default();
            }

            typeId = getTypeId(value);
            object obj = value;

            if (typeId == TypeId.None)
            {
                typeId = TypeId.String;
                return Encoding.ASCII.GetBytes(value.json());
            }

            else if (value != null)
                switch (typeId)
                {
                    case TypeId.Boolean: return BitConverter.GetBytes((bool)obj);

                    case TypeId.Short: return BitConverter.GetBytes((short)obj);
                    case TypeId.UnsignedShort: return BitConverter.GetBytes((ushort)obj);

                    case TypeId.Integer: return BitConverter.GetBytes((int)obj);
                    case TypeId.UnsignedInteger: return BitConverter.GetBytes((uint)obj);

                    case TypeId.Float: return BitConverter.GetBytes((float)obj);
                    case TypeId.Double: return BitConverter.GetBytes((double)obj);

                    case TypeId.String: return Encoding.ASCII.GetBytes((string)obj);
                    case TypeId.Char: return BitConverter.GetBytes((char)obj);

                    default: return @default();
                }
            else return @default();

            byte[] @default() => new byte[0];
        }

        public static T? fromBytes<T>(byte[] bytes)
        {
            switch (getTypeId(default(T)))
            {
                case TypeId.Boolean: return (T)(object)BitConverter.ToBoolean(bytes);

                case TypeId.Short: return (T)(object)BitConverter.ToInt16(bytes);
                case TypeId.UnsignedShort: return (T)(object)(BitConverter.ToInt16(bytes) + short.MaxValue + 1);

                case TypeId.Integer: return (T)(object)BitConverter.ToInt32(bytes);
                case TypeId.UnsignedInteger: return (T)(object)BitConverter.ToInt32(bytes).toUInt();

                case TypeId.Float: return (T)(object)BitConverter.ToSingle(bytes);
                case TypeId.Double: return (T)(object)BitConverter.ToDouble(bytes);

                case TypeId.String: return (T)(object)Encoding.ASCII.GetString(bytes);
                case TypeId.Char: return (T)(object)BitConverter.ToChar(bytes);

                default: return Encoding.ASCII.GetString(bytes).json<T>();
            }
        }

        public static TypeId getTypeId<T>(T value)
        {
            // Boolean
            if (value is bool) return TypeId.Boolean;

            // Short
            if (value is short) return TypeId.Short;
            else if (value is ushort) return TypeId.UnsignedShort;

            // Integer
            if (value is int) return TypeId.Integer;
            else if (value is uint) return TypeId.UnsignedInteger;

            // Float
            else if (value is float) return TypeId.Float;

            // Double
            else if (value is double) return TypeId.Double;

            // String + Char
            else if (value is string) return TypeId.String;
            else if (value is char) return TypeId.Char;

            return TypeId.None;
        }

        public enum TypeId
        {
            None = -69,

            Boolean = 0,

            Short = 1,
            UnsignedShort = -1,

            Integer = 2,
            UnsignedInteger = -2,

            Float = 3,
            Double = -3,

            String = 4,
            Char = -4
        }
        #endregion
    }
}