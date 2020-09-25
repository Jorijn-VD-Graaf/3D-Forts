using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Lobby
{
    public string hostIP;
    public string name;
    public int playersConnected;
    public int maxPlayers;
    public bool passProtected;
    public GameObject uiElement;
    public Map map;
    public byte[] password;
    public List<Player> spectators = new List<Player>();

    public Lobby(string hostIP, string name, int playersConnected, int maxPlayers, bool passProtected)
    {
        this.hostIP = hostIP;
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        this.passProtected = passProtected;
    }
    public Lobby(string name, int playersConnected, int maxPlayers, Map map, byte[] password)
    {
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        passProtected = true;
        this.map = map;
        this.password = password;
    }
    public Lobby(string name, int playersConnected, int maxPlayers, Map map)
    {
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        passProtected = false;
        this.map = map;
    }
    public Lobby(Packet packet, int type)
    {
        if (type == (int)Packets.lobbyUpdate|| type == (int)Packets.MapDownloadRequest)
        {
            if (type == (int)Packets.MapDownloadRequest)
            {
                map = new Map(packet.ReadString(), packet.ReadString(), packet.ReadString(), packet.ReadBytes(packet.ReadInt()), packet.ReadInt(), packet.ReadInt(), packet.ReadBytes(packet.ReadInt()));
            }
            int teamCount = packet.ReadInt();
            for (int i = 0; i < teamCount; i++)
            {
                int teamSize = packet.ReadInt();
                List<Player> team = new List<Player>();
                map.teams.Add(team);
                for (int i1 = 0; i1 < teamSize; i1++)
                {
                    string name = packet.ReadString();
                    if(name == "null")
                    {
                        team.Add(null);
                    }
                    else
                    {
                        team.Add(new Player(name, packet.ReadInt(), packet.ReadInt(), packet.ReadString()));
                    }
                }
            }
            int amountOfSpectators = packet.ReadInt();
            for (int i = 0; i < amountOfSpectators; i++)
            {
                spectators.Add(new Player(packet.ReadString(), packet.ReadInt(), packet.ReadInt(), packet.ReadString()));
            }
            name = packet.ReadString();
            map.guid = new Guid(packet.ReadString());
        }
    }
    /*
    public Lobby(string hostIP, string name, int playersConnected, int maxPlayers, bool passProtected, List<List<Player>> teams)
    {
        this.hostIP = hostIP;
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        this.passProtected = passProtected;
        this.teams = teams;
    }
    */
    public Packet ToPacket(int type)
    {
        if (type == (int)Packets.LobbyInfoRequest)
        {
            using (Packet packet = new Packet(type))
            {
                packet.Write(name);
                packet.Write(playersConnected);
                packet.Write(maxPlayers);
                packet.Write(passProtected);
                return packet;
            }
        }
        if (type == (int)Packets.lobbyUpdate|| type == (int)Packets.MapDownloadRequest)
        {
            using (Packet packet = new Packet(type))
            {
                if (type == (int)Packets.MapDownloadRequest)
                {
                    SerializedMap map = new SerializedMap(this.map);
                    packet.Write(map.name);
                    packet.Write(map.guid);
                    packet.Write(map.teamString);
                    packet.Write(map.mapdata.Length);
                    packet.Write(map.mapdata);
                    packet.Write(map.thumbnail.x);
                    packet.Write(map.thumbnail.y);
                    packet.Write(map.thumbnail.bytes.Length);
                    packet.Write(map.thumbnail.bytes);
                }

                packet.Write(map.teams.Count);
                foreach (List<Player> item in map.teams)
                {
                    packet.Write(item.Count);
                    foreach (Player player in item)
                    {
                        if (player != null)
                        {
                            packet.Write(player.name);
                            packet.Write(player.losses);
                            packet.Write(player.wins);
                            packet.Write(player.rank);
                        }
                        else
                        {
                            packet.Write("null");
                        }
                    }
                }
                packet.Write(spectators.Count);
                foreach (Player player in spectators)
                {
                    packet.Write(player.name);
                    packet.Write(player.losses);
                    packet.Write(player.wins);
                    packet.Write(player.rank);
                }
                packet.Write(name);
                packet.Write(map.guid.ToString());
                return packet;
            }
        }
        Debug.LogError("invalid packet type");
        return null;
    }
}
