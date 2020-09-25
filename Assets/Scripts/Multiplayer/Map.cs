using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class Map
{
    public GameObject[] mapdata;
    public List<List<Player>> teams = new List<List<Player>>();
    public Sprite thumbnail;
    public string name;
    public Guid guid;
    public string teamString = "";
    public Map(string name, Sprite thumbnail, List<List<Player>> teams, Guid Guid)
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
        this.guid = Guid;
    }
    public Map(SerializedMap map)
    {
        Texture2D tex = new Texture2D(map.thumbnail.x, map.thumbnail.y);
        ImageConversion.LoadImage(tex, map.thumbnail.bytes);
        thumbnail = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one);
        guid = new Guid(map.guid);
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
    public Map(string name, string guid, string teamString, byte[] mapData, int thumbnailX, int thumbnailY, byte[] thumbnailBytes)
    {
        Texture2D tex = new Texture2D(thumbnailX, thumbnailY);
        ImageConversion.LoadImage(tex, thumbnailBytes);
        thumbnail = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one);
        this.guid = new Guid(guid);
        this.name = name;
        this.teamString = teamString;
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
        using (FileStream fs = File.Create(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "3D Forts", "Maps"), $"{name}_{guid}.json")))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SerializedMap(this)));
            fs.Write(bytes, 0, bytes.Length);
        }
        Debug.Log($"Saved downloaded map: {name}");
    }
}

[Serializable]
public class SerializedMap
{
    public string name;
    public string guid;
    public string teamString;
    public byte[] mapdata;
    public SerializedTexture thumbnail;

    public SerializedMap(Map map)
    {
        thumbnail = new SerializedTexture(ImageConversion.EncodeToPNG(map.thumbnail.texture), map.thumbnail.texture.width, map.thumbnail.texture.height);
        guid = map.guid.ToString();
        name = map.name;
        teamString = map.teamString;
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
