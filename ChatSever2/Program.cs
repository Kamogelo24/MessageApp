using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ChatServer2.Net.IO;

namespace ChatServer2
{
    class Program
    {
        private static readonly Dictionary<Guid, Client> _users = new();
        private static TcpListener? _listener;
        private static bool _isRunning;

        static void Main()
        {
            Console.Title = "Chat Server";
            Console.WriteLine("Starting server...");
            Console.CancelKeyPress += (sender, e) => Shutdown();

            try
            {
                // Auto-detect IPv4 address
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress == null)
                {
                    Console.WriteLine("No IPv4 address found! Falling back to manual IP.");
                    Console.Write("Enter server IP manually (e.g., 192.168.1.100): ");
                    var manualIp = Console.ReadLine();
                    if (!IPAddress.TryParse(manualIp, out ipAddress))
                    {
                        Console.WriteLine("Invalid IP address. Shutting down.");
                        return;
                    }
                }

                Console.WriteLine($"Using IP: {ipAddress}");
                _listener = new TcpListener(ipAddress, 7891);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine($"Server started on {_listener.LocalEndpoint}");
                Console.WriteLine("Waiting for client connections...");

                var acceptThread = new Thread(AcceptClients);
                acceptThread.Start();

                while (_isRunning)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server Error: {ex.Message}");
                Shutdown();
            }
        }

        private static void AcceptClients()
        {
            while (_isRunning && _listener != null)
            {
                try
                {
                    var client = new Client(_listener.AcceptTcpClient());
                    lock (_users)
                    {
                        _users.Add(client.UID, client);
                        BroadcastUserConnected(client.Username, client.UID);
                    }
                    BroadcastUserList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Accept error: {ex.Message}");
                }
            }
        }

        public static void BroadcastUserConnected(string username, Guid userId)
        {
            foreach (var user in _users.Values)
            {
                if (user.UID != userId)
                {
                    try
                    {
                        var packet = new PacketBuilder();
                        packet.WriteOpCode(1);
                        packet.WriteMessage(username);
                        packet.WriteMessage(userId.ToString());
                        user.SendPacket(packet.GetPacketBytes());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}] Error sending user connected: {ex.Message}");
                    }
                }
            }
        }

        public static void BroadcastMessage(string message, Guid senderId, Guid? recipientId = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            Console.WriteLine($"[{DateTime.Now}] Broadcasting: {message}");

            List<Client> recipients;
            lock (_users)
            {
                recipients = recipientId.HasValue
                    ? (_users.TryGetValue(recipientId.Value, out var recipient)
                        ? new List<Client> { recipient }
                        : new List<Client>())
                    : new List<Client>(_users.Values.Where(u => u.UID != senderId));
            }

            foreach (var user in recipients)
            {
                try
                {
                    var packet = new PacketBuilder();
                    packet.WriteOpCode(5);
                    packet.WriteMessage(senderId.ToString());
                    packet.WriteMessage(message);
                    packet.WriteMessage(DateTime.Now.ToString("o"));
                    user.SendPacket(packet.GetPacketBytes());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Error sending to {user.Username}: {ex.Message}");
                    RemoveClient(user.UID);
                }
            }
        }

        public static void BroadcastUserList()
        {
            List<Client> recipients;
            lock (_users)
            {
                recipients = new List<Client>(_users.Values);
            }

            foreach (var user in recipients)
            {
                try
                {
                    var packet = new PacketBuilder();
                    packet.WriteOpCode(2);
                    packet.WriteMessage(_users.Count.ToString());

                    foreach (var usr in _users.Values)
                    {
                        packet.WriteMessage(usr.Username);
                        packet.WriteMessage(usr.UID.ToString());
                    }
                    user.SendPacket(packet.GetPacketBytes());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Error broadcasting user list: {ex.Message}");
                }
            }
        }

        public static void RemoveClient(Guid uid)
        {
            lock (_users)
            {
                if (_users.Remove(uid))
                {
                    BroadcastUserDisconnected(uid);
                    BroadcastUserList();
                }
            }
        }

        public static void BroadcastUserDisconnected(Guid userId)
        {
            foreach (var user in _users.Values)
            {
                try
                {
                    var packet = new PacketBuilder();
                    packet.WriteOpCode(10);
                    packet.WriteMessage(userId.ToString());
                    user.SendPacket(packet.GetPacketBytes());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Error sending user disconnected: {ex.Message}");
                }
            }
        }

        private static void Shutdown()
        {
            if (!_isRunning) return;
            _isRunning = false;

            Console.WriteLine("Shutting down server...");
            _listener?.Stop();

            lock (_users)
            {
                foreach (var user in _users.Values)
                {
                    user.Disconnect();
                }
                _users.Clear();
            }

            Console.WriteLine("Server stopped.");
        }
    }
}