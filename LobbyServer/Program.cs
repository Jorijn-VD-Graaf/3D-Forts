using System;
using System.Collections.Generic;
using System.Net;

namespace LobbyServer
{
    class Server
    {
        static HttpListener listener;
        static Dictionary<string, Lobby> lobbyList = new Dictionary<string, Lobby>();
        public static void Main()
        {
            try
            {

                listener = new HttpListener();
                listener.Prefixes.Add("http://192.168.1.100/");
                listener.Start();
                Process();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }
        public static void Process()
        {
            while (true)
            {

                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                System.IO.Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                HttpListenerResponse response = context.Response;
                string responseString;
                byte[] buffer;
                System.IO.Stream output;
                switch (request.Headers.Get("type"))
                {
                    case "LobbyCreate":
                        string name = reader.ReadLine();
                        int currentPlayers = int.Parse(reader.ReadLine());
                        int maxPlayers = int.Parse(reader.ReadLine());
                        string ip = reader.ReadLine();
                        bool passProtected = bool.Parse(reader.ReadLine());
                        lobbyList.Remove(ip);
                        lobbyList.Add(ip, new Lobby(name, currentPlayers, maxPlayers, ip, passProtected));
                        responseString = string.Empty;
                        response.StatusCode = 201;
                        response.StatusDescription = "Created";
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        Console.WriteLine($"Lobby created by: {context.Request.RemoteEndPoint.ToString()}, {name} {currentPlayers} {maxPlayers} {passProtected}");
                        Console.WriteLine($"There are now: {lobbyList.Values.Count} lobbies");
                        break;
                    case "GetLobbyList":
                        responseString = "";
                        foreach (Lobby lobby in lobbyList.Values)
                        {
                            responseString += $"{lobby.name}\n{lobby.currentPlayers}\n{lobby.maxPlayers}\n{lobby.ip}\n{lobby.passProtected}\n";
                        }
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        Console.WriteLine($"Lobby list request from: {context.Request.RemoteEndPoint.ToString()}");
                        break;
                    case "CloseLobby":
                        lobbyList.Remove(reader.ReadLine());
                        responseString = string.Empty;
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        Console.WriteLine($"Lobby closed by: {context.Request.RemoteEndPoint.ToString()}");
                        Console.WriteLine($"There are now: {lobbyList.Values.Count} lobbies");
                        break;
                }
                body.Close();
                reader.Close();
            }
        }
    }
    class Lobby
    {
        public string name;
        public int currentPlayers;
        public int maxPlayers;
        public string ip;
        public bool passProtected;
        public Lobby(string name, int currentPlayers, int maxPlayers, string ip, bool passProtected)
        {
            this.name = name;
            this.currentPlayers = currentPlayers;
            this.maxPlayers = maxPlayers;
            this.ip = ip;
            this.passProtected = passProtected;
        }
    }
}

