using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Open.Nat;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Server : MonoBehaviour
{
    /// <summary>Maximum players allowed in lobby</summary>
    public int maxPlayers;
    public int port;
    /// <summary>List of connected clients</summary>
    public Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public Dictionary<int, PacketHandler> packetHandlers;
    private TcpListener tcpListener;
    public Lobby lobby;
    public Client client;
    public string externalIp = null;
    public bool openNat = false;
    public bool running = false;
    public int visibilty = 0;
    public bool forceLocal = false;

    #region Startup
    /// <summary>Starts server and opens port</summary>
    public async void StartServer()
    {
        running = true;
        Debug.Log("Starting server...");
        if (!forceLocal)
        {
            await OpenPort();
            tcpListener = new TcpListener(IPAddress.Any, port);
        }
        else
        {
            Debug.LogWarning("Force local mode is turned on, online play is not availble");
            IPAddress localIP = null;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address;
                externalIp = endPoint.ToString().Split(':')[0];
            }
            tcpListener = new TcpListener(localIP, port);
        }
        InitializeServerData();
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Server started on port {port}.");
    }
    /// <summary>Opens port via open.nat</summary>
    public async Task OpenPort()
    {
        try
        {
            var nat = new NatDiscoverer();
            var cts = new CancellationTokenSource(5000);
            var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            var ip = await device.GetExternalIPAsync();

            Debug.Log($"Your IP: {ip}");
            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, 0, "3D forts"));
            Debug.Log($"Opened port: {port}");
            externalIp = ip.ToString();
            openNat = true;
        }
        catch (Exception e)
        {
            openNat = false;
            Debug.LogWarning($"Error while opening nat: {e}");
        }
    }
    /// <summary>Sets up packet handlers</summary>
    private void InitializeServerData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)Packets.welcome, HandleWelcome },
                { (int)Packets.MapDownloadRequest, HandleMapDownload },
                { (int)Packets.SlotSwitch, HandleSlotSwitch }
            };
        Debug.Log("Initialized packet handlers");
    }
    #endregion

    #region TCP methods
    /// <summary>Runs whenever a client tries to connect</summary>
    private void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");
        int playerCount = 0;
        foreach (List<Player> teams in lobby.map.teams)
        {
            foreach (Player player in teams)
            {
                if (player != null)
                {
                    playerCount++;
                }
            }
        }
        int i = 1;
        /*
    bool broken = false;
    print(playerCount);
    foreach (List<Player> teams in lobby.map.teams)
    {
    if (broken)
    {
        break;
    }
    foreach (Player player in teams)
    {
        if (broken)
        {
            break;
        }
        if (player.id == i)
        {
            i++;
        }
        else
        {
            broken = true;
        }
    }
    }
    */
        clients.Add(i, new ServerClient(i, this));
        clients[i].tcp.Connect(_client);
        Debug.Log($"{_client.Client.RemoteEndPoint} has been given client id: {i}");
        //if (visibilty == 0)
        //{
        //    Disconnect(i, "Server set to private");
        //}
        return;
    }

    /// <summary>Send tcp data to all connected clients</summary>
    /// <param name="packet">The packet to send</param>
    private void SendTCPDataToAll(Packet packet)
    {
        for (int i = 1; i <= clients.Count; i++)
        {
            if (clients[i].tcp.socket != null)
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
        Debug.Log($"Sending welcome to client {toClient}");
        try
        {
            using (Packet packet = new Packet((int)Packets.welcome))
            {
                packet.Write(toClient);
                clients[toClient].tcp.SendData(packet);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error with sending welcome to client {toClient}: {e}");
        }
    }
    public void SendLobbyUpdate()
    {
        UnityMainThread.wkr.AddJob(() =>
        {
            client.mainMenu.RefreshLobby(lobby);
        });
        try
        {
            Debug.Log("Sending Lobby Update");
            using (Packet packet = new Packet((int)Packets.lobbyUpdate))
            {
                packet.Write(lobby.map.guid.ToString());
                packet.Write(lobby.name);
                foreach (List<Player> team in lobby.map.teams)
                {
                    foreach (Player player in team)
                    {
                        if (player != null)
                        {
                            packet.Write(1);
                            packet.Write(player.name);
                            packet.Write(player.losses);
                            packet.Write(player.wins);
                            packet.Write(player.rank);
                        }
                        else
                        {
                            packet.Write(0);
                        }
                    }
                    packet.Write(2);
                }
                packet.Write(3);
                packet.Write(lobby.spectators.Count);
                foreach (Player player in lobby.spectators)
                {
                    packet.Write(player.name);
                    packet.Write(player.losses);
                    packet.Write(player.wins);
                    packet.Write(player.rank);
                }
                SendTCPDataToAll(packet);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error with sending lobby update: {e}");
        }
    }
    public void Disconnect(int id)
    {
        clients[id].tcp.socket.GetStream().Close();
        clients[id].tcp.socket.Close();
        clients.Remove(id);
    }
    public void Disconnect(int id, string reason)
    {
        using (Packet packet = new Packet((int)Packets.disconnect))
        {
            packet.Write(reason);
            clients[id].tcp.SendData(packet);
        }
        StartCoroutine(DisconnectWithReason(id));

    }
    IEnumerator DisconnectWithReason(int id)
    {
        yield return new WaitForSeconds(0.5F);
        Disconnect(id);
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
        Debug.Log("Handeling Welcome");
        try
        {
            int _clientIdCheck = packet.ReadInt();
            Player player = new Player(packet);
            player.id = _clientIdCheck;
            for (int i1 = 0; i1 < lobby.map.teams.Count; i1++)
            {
                List<Player> team = lobby.map.teams[i1];
                Player[] array = team.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    Player playerr = array[i];
                    if (playerr == null)
                    {
                        lobby.map.teams[i1][i] = player;
                    }
                }
            }
            Debug.Log($"{clients[fromClient].tcp.socket.Client.RemoteEndPoint}/{player.name} connected successfully and is now client {fromClient}.");
            SendLobbyUpdate();
            if (fromClient != _clientIdCheck)
            {
                Debug.LogError($"Player {fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error with sending welcome: {e}");
        }
    }
    private void HandleSlotSwitch(int fromClient, Packet packet)
    {
        try
        {
            Debug.Log("Handeling Slot Switch");
            int team = packet.ReadInt();
            int slot = packet.ReadInt();

            Player player = lobby.map.teams.SelectMany(x => x).ToList().Where(x => x != null).ToList().Where(x => x.id == fromClient).ToList()[0];
            if (lobby.map.teams[team][slot] == null)
            {
                foreach (List<Player> teamList in lobby.map.teams)
                {
                    for (int i = 0; i < teamList.Count; i++)
                    {
                        if (teamList[i] != null)
                        {
                            if (teamList[i].id == fromClient)
                            {
                                teamList[i] = null;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < lobby.spectators.Count; i++)
                {
                    if (lobby.spectators[i].id == fromClient)
                    {
                        lobby.spectators.RemoveAt(i);
                        break;
                    }
                }
                lobby.map.teams[team][slot] = player;
                SendLobbyUpdate();
            }
        }
        catch (Exception e)
        {
 
        }
    }
    private void HandleMapDownload(int fromClient, Packet _packet)
    {
        //clients[fromClient].tcp.SendData(lobby.ToPacket((int)Packets.MapDownloadRequest));
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
                Debug.LogError($"Error receiving TCP data: {_ex}");
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

