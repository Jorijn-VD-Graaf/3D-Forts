using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Open.Nat;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    /// <summary>Maximum players allowed in lobby</summary>
    public int maxPlayers;
    public int port;
    public int lobbyDetailsPort = 42072;
    private string exteranlIp;
    /// <summary>List of connected clients</summary>
    public Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public Dictionary<int, PacketHandler> packetHandlers;
    private TcpListener tcpListener;
    private TcpListener lobbyTcpListener;
    public Lobby lobby;
    public Client client;
    public string externalIp;

    #region Startup
    /// <summary>Starts server and opens port</summary>
    public async void StartServer()
    {
        Debug.Log("Starting server...");
        await OpenPort();

        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        lobbyTcpListener = new TcpListener(IPAddress.Any, lobbyDetailsPort);
        lobbyTcpListener.Start();
        lobbyTcpListener.BeginAcceptTcpClient(LobbyTCPConnectCallback, null);

        client.StartClient();
        client.RegisterLobby(exteranlIp);
        Debug.Log($"Server started on port {port}.");
    }
    /// <summary>Opens port via open.nat</summary>
    public async Task OpenPort()
    {
        var nat = new NatDiscoverer();
        var cts = new CancellationTokenSource(5000);
        var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
        var ip = await device.GetExternalIPAsync();

        Debug.Log($"Your IP: {ip}");

        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, 0, "3D forts"));
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, lobbyDetailsPort, lobbyDetailsPort, 0, "3D forts Lobby"));
        Debug.Log($"Opened port: {port}");
        exteranlIp = ip.ToString();
    }
    /// <summary>Sets up packet handlers</summary>
    private void InitializeServerData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)Packets.welcome, HandleWelcome }
            };
        Debug.Log("Initialized packet handlers");
    }
    #endregion

    #region TCP methods
    /// <summary>Runs whenever a client tries to connect</summary>
    private void LobbyTCPConnectCallback(IAsyncResult _result)
    {
        try
        {
            TcpClient _client = lobbyTcpListener.EndAcceptTcpClient(_result);
            lobbyTcpListener.BeginAcceptTcpClient(LobbyTCPConnectCallback, null);
            Debug.Log($"Incoming Lobby info request from {_client.Client.RemoteEndPoint}...");
            ServerClient client = new ServerClient(0, this);
            client.tcp.Connect(_client, true);
            client.tcp.SendData(lobby.ToPacket((int)Packets.LobbyInfoRequest));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");
        int playerCapacity = 0;
        foreach (List<Player> teams in lobby.teams)
        {
            playerCapacity += teams.Count;
        }
        if (playerCapacity <= 0)
        {
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
            return;
        } 
        for (int i = 0; i < playerCapacity; i++)
        {
            foreach (List<Player> teams in lobby.teams)
            {
                foreach (Player player in teams)
                {
                    if (player.id == i)
                    {
                        i++;
                        continue;
                    }
                }
            }
            clients.Add(i, new ServerClient(i, this));
            clients[i].tcp.Connect(_client);
            return;
        }
    }

    /// <summary>Send tcp data to single client</summary>
    /// <param name="toClient">The ID of the client to send to</param>
    /// <param name="packet">The packet to send</param>
    private void SendTCPData(int toClient, Packet packet)
    {
        clients[toClient].tcp.SendData(packet);
    }

    /// <summary>Send tcp data to all connected clients</summary>
    /// <param name="packet">The packet to send</param>
    private void SendTCPDataToAll(Packet packet)
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.SendData(packet);
            }
        }
    }
    private void SendTCPDataToAllExecpt(int toClient, Packet packet)
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            if (i != toClient)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.SendData(packet);
                }
            }
        }
    }
    #endregion

    #region Packet senders
    /// <summary>
    /// Send welcome message
    /// </summary>
    /// <param name="toClient">The ID of the client to send to</param>
    /// <param name="msg">The message</param>
    public void SendWelcome(int toClient)
    {
        Packet packet = lobby.ToPacket((int)Packets.welcome);
        packet.Write(toClient);
        SendTCPData(toClient, packet);
    }
    public void SendLobbyUpdate()
    {
        SendTCPDataToAll(lobby.ToPacket((int)Packets.lobbyUpdate));
    }
    #endregion

    #region packet handlers
    /// <summary>
    /// Handles welcome packet
    /// </summary>
    /// <param name="fromClient">Client ID packet orignated from</param>
    /// <param name="packet">The recieved packet</param>
    public void HandleWelcome(int fromClient, Packet packet)
    {
        Player player = new Player(packet);
        int _clientIdCheck = packet.ReadInt();
        player.id = _clientIdCheck;
        foreach(List<Player> team in lobby.teams)
        {
            foreach (Player playerr in team)
            {
                if (playerr == null)
                {
                    team.Add(player);
                }
            }
        
        }
        Debug.Log($"{clients[fromClient].tcp.socket.Client.RemoteEndPoint}/{player.name} connected successfully and is now player {fromClient}.");
        SendLobbyUpdate();
        if (fromClient != _clientIdCheck)
        {
            Debug.LogError($"Player {fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
    }
    #endregion
}
public class ServerClient
{
    public static int dataBufferSize = 4096;

    public int id;
    public TCP tcp;

    public ServerClient(int _clientId, Server server)
    {
        id = _clientId;
        tcp = new TCP(id, server);
    }

    public class TCP
    {
        public TcpClient socket;
        public Server server;
        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id, Server server)
        {
            this.server = server;
            id = _id;
        }

        public void Connect(TcpClient _socket, bool lobby)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;
            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            server.SendWelcome(id);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    _packet.WriteLength();
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
            }
        }
        /// <summary>
        /// Reads packet and sends too appropaite packet handler
        /// </summary>
        /// <param name="data">the packet in a byte array</param>
        /// <returns></returns>
        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            //first 4 bytes are packet size
            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                using (Packet packet = new Packet(packetBytes))
                {
                    //reads packet type and runs approipete handler
                    int packetId = packet.ReadInt();
                    server.packetHandlers[packetId](id, packet);
                }

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    //not the entire packet has been recieved yet
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                //corrupted packet
                return true;
            }

            return false;
        }
    }
}

