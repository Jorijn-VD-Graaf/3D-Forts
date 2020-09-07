using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public class Map
{
    public GameObject[] mapdata;
    public List<List<Player>> teams = new List<List<Player>>();
    public Sprite thumbnail;
    public string name;
    public GUID UUID;
    public string teamString;
    public Map(string name, string teamString, Sprite thumbnail, List<List<Player>> teams)
    {
        this.name = name;
        this.teamString = teamString;
        this.thumbnail = thumbnail;
        this.teams = teams;
        UUID = GUID.Generate();
    }
}
