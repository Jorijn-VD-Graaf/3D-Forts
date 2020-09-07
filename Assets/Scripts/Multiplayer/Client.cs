﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Security.Cryptography;

public class Client : MonoBehaviour
{
    /// <summary>Size in bytes of the data buffer</summary>
    public static int dataBufferSize = 4096;

    public InputField ipInput;

    private string ip;
    private int port = 42069;
    private int lobbyPort = 42070;
    private int lobbyPort2 = 42071;
    private int lobbyDetailsPort = 42072;
    public string lobbyIp;
    public int myId = 0;
    public TCP tcp;
    public mainMenuScript mainMenu;
    private List<string> serverIps = new List<string>();
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    public List<Lobby> lobbies = new List<Lobby>();
    public Lobby currentLobby;

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
            { (int) Packets.requestLobbyList, HandleLobbyRequest},
            { (int) Packets.LobbyInfoRequest, HandleLobbyInfo},
            { (int) Packets.lobbyUpdate, HandleLobbyRefresh},
        };
        print("Initzailzed packet handlers.");
    }
    #endregion

    #region TCP methods
    public void Disconnect()
    {
        tcp.socket.Dispose();
    }
    public void GetLobbyList()
    {
        tcp.Connect(lobbyIp, lobbyPort);
    }
    public void RegisterLobby(string ip)
    {
        tcp.Connect(lobbyIp, lobbyPort2);
        StartCoroutine(ExampleCoroutine(ip));

    }
    public void JoinLobby(Lobby lobby)
    {
        tcp.Connect(ip, port);
        currentLobby = lobby;
    }
    IEnumerator ExampleCoroutine(string ip)
    {
        yield return new WaitForSeconds(0.5F);
        using (Packet _packet = new Packet((int)Packets.registerLobby))
        {
            print(ip);
            _packet.Write(ip);
            tcp.SendData(_packet);
        }

    }
    #endregion

    #region Packet senders
    #endregion

    #region Packet handlers
    /// <summary>
    /// Handles welcome packet and responds with ID
    /// </summary>
    /// <param name="packet">Recieved packet</param>
    public void HandleWelcome(Packet packet)
    {
        HandleLobbyRefresh(packet);
        int _myId = packet.ReadInt();
        myId = _myId;
        Packet _packet = mainMenu.player.ToPacket((int)Packets.welcome);
        _packet.Write(myId);
        tcp.SendData(_packet);
    }
    public void HandleLobbyRefresh(Packet packet)
    {
        currentLobby = new Lobby(packet,(int)Packets.lobbyUpdate);
        mainMenu.RefreshLobby(currentLobby);
    }
    public void HandleLobbyRequest(Packet packet)
    {
        int amount = packet.ReadInt();
        for (int i = 0; i < amount; i++)
        {
            string ip = packet.ReadString();
            string[] ipSplit = ip.Split(':');
            serverIps.Add(ipSplit[0]);
        }
        NextLobby();
    }
    private void NextLobby()
    {
        tcp.socket.Dispose();
        print(serverIps[0]);
        try
        {
            tcp.Connect(serverIps[0], lobbyDetailsPort);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        serverIps.RemoveAt(0);
    }
    public void HandleLobbyInfo(Packet packet)
    {
        Lobby lobbyInfo = new Lobby("", packet.ReadString(), packet.ReadInt(), packet.ReadInt(), packet.ReadBool());
        print(lobbyInfo.hostIP + lobbyInfo.name);
        mainMenu.addLobby(lobbyInfo);
        lobbies.Add(lobbyInfo);
        NextLobby();
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
            print($"connected to {socket.Client.RemoteEndPoint}");
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
            catch
            {

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
