using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MessageApp.Net.IO  // For ChatServer2 project
// namespace MessageApp.Net.IO  // For MessageApp project
{
    public class PacketReader : BinaryReader
    {
        private readonly NetworkStream _ns;

        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            var length = ReadInt32();
            var msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            return Encoding.ASCII.GetString(msgBuffer);
        }
    }
}