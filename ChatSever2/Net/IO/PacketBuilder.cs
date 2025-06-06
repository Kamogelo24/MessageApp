using System;
using System.IO;
using System.Text;

namespace ChatServer2.Net.IO
{
    public class PacketBuilder
    {
        private readonly MemoryStream _ms = new();

        public void WriteOpCode(byte opcode) => _ms.WriteByte(opcode);

        public void WriteMessage(string msg)
        {
            var msgBytes = Encoding.ASCII.GetBytes(msg);
            _ms.Write(BitConverter.GetBytes(msgBytes.Length), 0, 4);
            _ms.Write(msgBytes, 0, msgBytes.Length);
        }

        public byte[] GetPacketBytes() => _ms.ToArray();
    }
}