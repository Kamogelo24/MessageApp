using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer2.Net.IO;

namespace ChatServer2
{
    public class Client
    {
        public string Username { get; set; } = string.Empty;
        public Guid UID { get; } = Guid.NewGuid();
        public TcpClient ClientSocket { get; }
        private readonly PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client ?? throw new ArgumentNullException(nameof(client));
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}] {Username} connected (ID: {UID})");
            _ = Task.Run(ProcessAsync);
        }

        private async Task ProcessAsync()
        {
            try
            {
                while (ClientSocket.Connected)
                {
                    var opcode = await Task.Run(() => _packetReader.ReadByte());
                    switch (opcode)
                    {
                        case 5:
                            var (senderId, message, timestamp) = _packetReader.ReadMessagePacket();
                            Console.WriteLine($"[{DateTime.Now}] Message from {Username}: {message}");
                            Program.BroadcastMessage(message, UID, null);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{UID}] Error: {ex.Message}");
                Disconnect();
            }
        }

        public void SendPacket(byte[] packet)
        {
            try
            {
                ClientSocket.Client.Send(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{UID}] Send failed: {ex.Message}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                ClientSocket?.Close();
                Program.RemoveClient(UID);
                Console.WriteLine($"[{UID}] Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{UID}] Disconnect error: {ex.Message}");
            }
        }
    }
}