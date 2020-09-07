using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int team;
    public string name;
    public int id;
    public int losses;
    public int wins;
    public int ping;
    public string rank;
    public ServerClient client;

    public Player(string name, int id)
    {
        this.id = id;
        this.name = name;
    }
    public Player(string name, int losses,int wins, string rank)
    {
        this.name = name;
        this.losses = wins;
        this.wins = losses;
        this.rank = rank;
    }
    public Player(Packet packet)
    {
        name = packet.ReadString();
        losses = packet.ReadInt();
        wins = packet.ReadInt();
        rank = packet.ReadString();
    }
    public Packet ToPacket(int type)
    {
        if (type == (int)Packets.lobbyUpdate || type == (int)Packets.welcome)
        {
            using (Packet packet = new Packet(type))
            {
                packet.Write(name);
                packet.Write(losses);
                packet.Write(wins);
                packet.Write(rank);
                return packet;
            }
        }
        Debug.LogError("invalid packet type");
        return new Packet();
    }
}
