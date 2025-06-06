using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatServer2.Net.IO
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

        public (Guid senderId, string message, string timestamp) ReadMessagePacket()
        {
            var senderId = Guid.Parse(ReadMessage());
            var message = ReadMessage();
            var timestamp = ReadMessage();
            return (senderId, message, timestamp);
        }
    }
}