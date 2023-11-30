using System.Net.Sockets;

namespace Walhalla
{
    public static class Networkf
    {
        /// <summary>
        /// Be aware that transferring the ownership means that closing/disposing the stream will also close the underlying socket.
        /// </summary>
        public static NetworkStream GetStream(this Socket socket) => new NetworkStream(socket, ownsSocket: true);
    }
}