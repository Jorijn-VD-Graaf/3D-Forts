using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    public GameObject[] mapdata;
    public List<List<Player>> teams = new List<List<Player>>();
    public Sprite thumbnail;
    public string name;
    public Guid UUID;
    public string teamString = "";
    public Map(string name, Sprite thumbnail, List<List<Player>> teams, Guid UUID)
    {
        this.name = name;
        for (int i = 0; i < teams.Count; i++)
        {
            if (i < teams.Count-1)
            {
                teamString += $"{teams[i].Count}v";
            }
            else
            {
                teamString += $"{teams[i].Count}";
            }

        }
        this.thumbnail = thumbnail;
        this.teams = teams;
        this.UUID = UUID;
    }
    public Map(SerializedMap map)
    {
        Texture2D tex = new Texture2D(map.thumbnail.x, map.thumbnail.y);
        ImageConversion.LoadImage(tex, map.thumbnail.bytes);
        thumbnail = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one);
        UUID = new Guid(map.UUID);
        name = map.name;
        teamString = map.teamString;
        char[] teamNumbers = teamString.Replace("v", string.Empty).ToCharArray();
        for (int i = 0; i < teamString.Replace("v", string.Empty).Length; i++)
        {
            int teamsize = int.Parse(teamNumbers[i].ToString());
            teams.Add(new List<Player>());
            for (int i2 = 0; i2 < teamsize; i2++)
            {
                teams[i].Add(null);
            }
        }
    }
}

[Serializable]
public class SerializedMap
{
    public string name;
    public string UUID;
    public string teamString;
    public List<Player> players = new List<Player>();
    public byte[] mapdata;
    public SerializedTexture thumbnail;

    public SerializedMap(Map map)
    {
        thumbnail = new SerializedTexture(ImageConversion.EncodeToPNG(map.thumbnail.texture), map.thumbnail.texture.width, map.thumbnail.texture.height);
        UUID = map.UUID.ToString();
        name = map.name;
        teamString = map.teamString;
        foreach (List<Player> team in map.teams)
        {
            for (int i = 0; i < team.Count-1; i++)
            {
                players.Add(team[i]);
            }
        }
    }
}
[Serializable]
public class SerializedTexture
{
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;
    [SerializeField]
    public byte[] bytes;

    public SerializedTexture(byte[] bytes, int x, int y)
    {
        this.bytes = bytes;
        this.x = x;
        this.y = y;
    }
}
