using System.Collections.Generic;
using UnityEngine;
using System;

public class Lobby
{
    public string hostIP;
    public string name;
    public bool passProtected;
    public Map map;
    public byte[] password;
    public List<Player> spectators = new List<Player>();

    public Lobby(string name, Map map)
    {
        this.name = name;
        this.map = map;
    }
    public Lobby(Packet packet, int type, Map map)
    {
        try
        {
            if (type == (int)Packets.lobbyUpdate)
            {
                name = packet.ReadString();
                /*
                if (type == (int)Packets.MapDownloadRequest)
                {
                    this.map = new Map(packet.ReadString(), packet.ReadString(), packet.ReadString(), packet.ReadBytes(packet.ReadInt()), packet.ReadInt(), packet.ReadInt(), packet.ReadBytes(packet.ReadInt()));
                }
                */
                this.map = map;
                int teamCounter = 0;
                int slotCounter = 0;
                bool notBroken = true;
                while (notBroken)
                {
                    int typee = packet.ReadInt();
                    switch (typee)
                    {
                        case 0:
                            map.teams[teamCounter][slotCounter] = null;
                            slotCounter++;
                            break;
                        case 1:
                            map.teams[teamCounter][slotCounter] = new Player(packet.ReadString(), packet.ReadInt(), packet.ReadInt(), packet.ReadString());
                            slotCounter++;
                            break;
                        case 2:
                            slotCounter = 0;
                            teamCounter++;
                            break;
                        case 3:
                            notBroken = false;
                            break;

                    }
                }
                //int amountOfSpectators = packet.ReadInt();
                //for (int i = 0; i < amountOfSpectators; i++)
                //{
                //spectators.Add(new Player(packet.ReadString(), packet.ReadInt(), packet.ReadInt(), packet.ReadString()));
                //}
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while parsing lobby from packet {e}");
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
        if (type == (int)Packets.lobbyUpdate || type == (int)Packets.MapDownloadRequest)
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
