namespace Walhalla
{
    public class ClientBase
    {
        protected Dictionary<uint, ClientBase> Registry;
        public uint UID;

        public ClientBase(uint uid, ref Dictionary<uint, ClientBase> registry)
        {
            $"+++ Connected [{uid}]".Log();

            Registry = registry;
            UID = uid;
        }

        public virtual void send(byte key, BufferType type, byte[]? bytes, bool tcp) { }
        public virtual void send<T>(byte key, T value, bool tcp) { }

        /// <summary> Handles incomming traffic </summary>
        public virtual void onReceive(byte key, BufferType type, byte[]? bytes, bool tcp)
        {
            $"{UID}> {(tcp ? "tcp" : "udp")}-package: {key} ({type}, {(bytes == null ? 0 : bytes.Length)})".Log();
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
}