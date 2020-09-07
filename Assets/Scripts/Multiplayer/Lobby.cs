using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Lobby
{
    public string hostIP;
    public string name;
    public int playersConnected;
    public int maxPlayers;
    public bool passProtected;
    public GameObject uiElement;
    public string gameMode;
    public List<List<Player>> teams = new List<List<Player>>();
    public Map map;
    public byte[] password;

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
        teams = map.teams;
        this.map = map;
        this.password = password;
    }
    public Lobby(string name, int playersConnected, int maxPlayers, Map map)
    {
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        passProtected = false;
        teams = map.teams;
        this.map = map;
    }
    public Lobby(Packet packet, int type)
    {
        if (type == (int)Packets.lobbyUpdate)
        {
            int teamCount = packet.ReadInt();
            for (int i = 0; i < teamCount; i++)
            {
                int teamSize = packet.ReadInt();
                List<Player> team = new List<Player>();
                teams.Add(team);
                for (int i1 = 0; i1 < teamSize; i1++)
                {
                    team.Add(new Player(packet.ReadString(), packet.ReadInt(), packet.ReadInt(), packet.ReadString()));
                }
            }
            name = packet.ReadString();
            map.UUID = new GUID(packet.ReadString());
        }
    }
    public Lobby(string hostIP, string name, int playersConnected, int maxPlayers, bool passProtected, List<List<Player>> teams)
    {
        this.hostIP = hostIP;
        this.name = name;
        this.playersConnected = playersConnected;
        this.maxPlayers = maxPlayers;
        this.passProtected = passProtected;
        this.teams = teams;
    }
    public Packet ToPacket(int type)
    {
        if (type == (int)Packets.LobbyInfoRequest)
        {
            using (Packet packet = new Packet((int)Packets.LobbyInfoRequest))
            {
                packet.Write(name);
                packet.Write(playersConnected);
                packet.Write(maxPlayers);
                packet.Write(passProtected);
                return packet;
            }
        }
        if (type == (int)Packets.lobbyUpdate)
        {
            using (Packet packet = new Packet((int)Packets.lobbyUpdate))
            {
                packet.Write(teams.Count);
                foreach (List<Player> item in teams)
                {
                    packet.Write(item.Count);
                    foreach (Player player in item)
                    {
                        packet.Write(player.name);
                        packet.Write(player.losses);
                        packet.Write(player.wins);
                        packet.Write(player.rank);
                    }
                }
                packet.Write(name);
                packet.Write(map.UUID.ToString());
                return packet;
            }
        }
        Debug.LogError("invalid packet type");
        return null;
    }
}
