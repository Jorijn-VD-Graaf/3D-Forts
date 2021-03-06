using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine.UIElements;
using System.Net.Http;

public class Client : MonoBehaviour
{
    /// <summary>Size in bytes of the data buffer</summary>
    public static int dataBufferSize = 4096;
    public InputField ipInput;
    private int port = 42069;
    public string lobbyIp;
    public int myId = -1;
    public TCP tcp;
    public mainMenuScript mainMenu;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    public List<Lobby> lobbies = new List<Lobby>();
    public Lobby lobby;
    public readonly HttpClient http = new HttpClient();

    #region Startup
    /// <summary>
    /// Starts client and attemps to connect to Lobby server
    /// </summary>
    public void StartClient()
    {
        tcp = new TCP();
        InitializeClientData();
    }
    /// <summary>Sets up packet handlers</summary>
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int) Packets.welcome, HandleWelcome},
            { (int) Packets.lobbyUpdate, HandleLobbyRefresh},
            { (int) Packets.MapDownloadRequest, HandleDownload},
            { (int) Packets.disconnect, HandleDisconnect},
        };
        Debug.Log("Initzailzed packet handlers.");
    }
    #endregion

    #region TCP methods
    public void Disconnect()
    {
        tcp.socket.Dispose();
    }
    public void JoinLobby(string ip)
    {
        tcp.Connect(ip, port);
    }
    #endregion

    #region Packet senders
    private void SendMapDownloadRequest(string guid)
    {
        using (Packet packet = new Packet((int)Packets.MapDownloadRequest))
        {
            packet.Write(guid);
            tcp.SendData(packet);
        }
    }
    public void SendSlotSwitch(int team, int slot)
    {
        using (Packet packet = new Packet((int)Packets.SlotSwitch))
        {
            packet.Write(team);
            packet.Write(slot);
            tcp.SendData(packet);
        }
    }
    #endregion

    #region Packet handlers
    /// <summary>
    /// Handles welcome packet and responds with ID
    /// </summary>
    /// <param name="packet">Recieved packet</param>
    public void HandleWelcome(Packet packet)
    {
        Debug.Log("Handeling Welcome");
        try
        {
            using (Packet _packet = new Packet((int)Packets.welcome))
            {
                int _myId = packet.ReadInt();
                myId = _myId;
                Debug.Log($"Assigned ID: {myId}");
                _packet.Write(myId);
                _packet.Write(mainMenu.player.name);
                _packet.Write(mainMenu.player.losses);
                _packet.Write(mainMenu.player.wins);
                _packet.Write(mainMenu.player.rank);
                tcp.SendData(_packet);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while handeling welcome: {e}");
        }
    }
    public void HandleDisconnect(Packet packet)
    {
        Debug.Log("Handeling Disconnect");
        string reason = packet.ReadString();
        mainMenu.ShowDisconnect(reason);
    }
    public void HandleLobbyRefresh(Packet packet)
    {
        Debug.Log("Handeling Lobby Refresh");
        try
        {
            //bool downloading = false;
            //Guid mapGuid = new Guid(packet.ReadString());
            /*
            foreach (Map map in mainMenu.maps)
            {
                if (map.guid != mapGuid)
                {
                    continue;
                }
                Debug.Log("Downloading map from server");
                SendMapDownloadRequest(mapGuid.ToString());
                downloading = true;
            }
            */
            //if(downloading == false)
            //{
            Guid mapid = Guid.Parse(packet.ReadString());
            lobby = new Lobby(packet, (int)Packets.lobbyUpdate, mainMenu.maps[mapid]);
            UnityMainThread.wkr.AddJob(() =>
            {
                mainMenu.RefreshLobby(lobby);
            });
            //}
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while handeling lobby refresh: {e}");
        }
    }
    private void HandleDownload(Packet packet)
    {
        Debug.Log("Handeling Map Download");
        //lobby = new Lobby(packet, (int)Packets.MapDownloadRequest);
        mainMenu.RefreshLobby(lobby);
    }
    #endregion

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        /// <summary>
        /// Attempts to connect to server
        /// </summary>
        /// <param name="ip">Server ip</param>
        /// <param name="port">Server port</param>
        public void Connect(string ip, int port)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            try
            {
                socket.BeginConnect(IPAddress.Parse(ip), port, ConnectCallBack, socket);
            }
            catch (SocketException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Runs when connection sucsesfull or time out
        /// </summary>
        /// <param name="result">result of the connection attempt</param>
        private void ConnectCallBack(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected)
            {
                return;
            }
            Debug.Log($"connected to {socket.Client.RemoteEndPoint}");
            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        /// <summary>
        /// sends data to server
        /// </summary>
        /// <param name="packet">packet to send</param>
        public void SendData(Packet packet)
        {
            packet.WriteLength();
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while sending TCP data: {e}");
            }
        }

        /// <summary>
        /// gets called when data is received
        /// </summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLenght = stream.EndRead(result);
                if (byteLenght <= 0)
                {
                    //corrupted packet
                    return;
                }
                byte[] data = new byte[byteLenght];
                Array.Copy(receiveBuffer, data, byteLenght);
                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                print(e);
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
                    packetHandlers[packetId](packet);
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

