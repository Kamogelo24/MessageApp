using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessageApp.Net.IO;
using MessageApp.mwm.model;

namespace MessageApp.Net
{
    public class Server
    {
        public Guid UID { get; private set; } = Guid.NewGuid();
        private TcpClient _client = new();
        private PacketReader? _packetReader;
        private string _username = string.Empty;
        private string _serverIp = "127.0.0.1";

        public event Action<UserModel>? UserConnectedEvent;
        public event Action<Guid>? UserDisconnectedEvent;
        public event Action<Guid, string, string>? MessageReceivedEvent;
        public bool IsConnected => _client?.Connected ?? false;

        public void ConnectToServer(string username, string serverIp)
        {
            if (!_client.Connected)
            {
                try
                {
                    _username = username;
                    _serverIp = serverIp;
                    _client = new TcpClient();
                    _client.Connect(_serverIp, 7891);
                    _packetReader = new PacketReader(_client.GetStream());

                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    connectPacket.WriteMessage(UID.ToString());
                    _client.Client.Send(connectPacket.GetPacketBytes());

                    _ = Task.Run(ReadPackets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection error: {ex.Message}");
                    Disconnect();
                }
            }
        }

        private void ReadPackets()
        {
            try
            {
                while (_client.Connected && _packetReader != null)
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 1: // User connected
                            var username = _packetReader.ReadMessage();
                            var uid = Guid.Parse(_packetReader.ReadMessage());
                            UserConnectedEvent?.Invoke(new UserModel
                            {
                                Username = username,
                                UID = uid
                            });
                            break;

                        case 2: // User list
                            var userCount = int.Parse(_packetReader.ReadMessage());
                            for (int i = 0; i < userCount; i++)
                            {
                                var uname = _packetReader.ReadMessage();
                                var userId = Guid.Parse(_packetReader.ReadMessage());
                                if (userId != UID)
                                {
                                    UserConnectedEvent?.Invoke(new UserModel
                                    {
                                        Username = uname,
                                        UID = userId
                                    });
                                }
                            }
                            break;

                        case 5: // Message
                            var senderId = Guid.Parse(_packetReader.ReadMessage());
                            var message = _packetReader.ReadMessage();
                            var timestamp = _packetReader.ReadMessage();
                            MessageReceivedEvent?.Invoke(senderId, message, timestamp);
                            break;

                        case 10: // User disconnected
                            var disconnectedUserId = Guid.Parse(_packetReader.ReadMessage());
                            UserDisconnectedEvent?.Invoke(disconnectedUserId);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading packets: {ex.Message}");
                Disconnect();
            }
        }

        public void SendMessageToServer(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                var packet = new PacketBuilder();
                packet.WriteOpCode(5);
                packet.WriteMessage(UID.ToString());
                packet.WriteMessage(message);
                packet.WriteMessage(DateTime.Now.ToString("o"));
                _client.Client.Send(packet.GetPacketBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                Disconnect();
            }
        }

        public void SendPrivateMessage(Guid recipientId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                var packet = new PacketBuilder();
                packet.WriteOpCode(5);
                packet.WriteMessage(UID.ToString());
                packet.WriteMessage(message);
                packet.WriteMessage(DateTime.Now.ToString("o"));
                packet.WriteMessage(recipientId.ToString());
                _client.Client.Send(packet.GetPacketBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending private message: {ex.Message}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_client.Connected)
                {
                    var packet = new PacketBuilder();
                    packet.WriteOpCode(10);
                    packet.WriteMessage(UID.ToString());
                    _client.Client.Send(packet.GetPacketBytes());
                }
                _client.Close();
                _packetReader = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnect: {ex.Message}");
            }
        }
    }
}