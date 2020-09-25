using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using System.IO;
using UnityEditor.Rendering;

public class mainMenuScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject optionsMenu;
    public GameObject mainMenu;
    public GameObject grahpicsMenu;
    public GameObject gameplayMenu;
    public InputField playerName;
    public Text winLoss;
    public InputField rank;
    public GameObject resolution;
    public GameObject fov;
    public GameObject scale;
    public Dropdown quality;
    public GameObject displayMode;
    public int fovInt;
    public float scaleInt;
    public Client client;
    public Server server;
    public GameObject lobbyPrefab;
    public Player player;
    public GameObject lobbySlotPrefab;
    public GameObject lobbyTeamPrefab;
    public GameObject lobbyMapPrefab;
    public Image lobbyMapThumbnail;
    public GameObject lobbyMapThumbnailGO;
    public GameObject lobbyMapScrollview;
    public TMP_Text selectedMap;
    public TMP_InputField lobbyName;
    public List<Map> maps = new List<Map>();
    public GameObject lobbyScreen;
    public GameObject lobbySelector;
    public GameObject playerCreator;
    public InputField playerCreatorr;
    public Sprite testMapSprite;
    public GameObject teamScrollViewContent;
    public GameObject mapSelectorScrollview;
    public Dictionary<string, string> folders = new Dictionary<string, string>();
    public GameObject spectators;
    public GameObject spectatorPrefab;
    public TMP_Dropdown publicictyChooser;

    #region start screen
    public void Start()
    {
        folders.Add("maps", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "3D Forts", "Maps"));
        if (PlayerPrefs.HasKey("UiScale"))
        {
            scaleInt = PlayerPrefs.GetFloat("UiScale");
            transform.gameObject.GetComponent<CanvasScaler>().scaleFactor = scaleInt;
        }
        if (PlayerPrefs.HasKey("PlayerName"))
        {

            player = new Player(PlayerPrefs.GetString("PlayerName"), PlayerPrefs.GetInt("playerLosses"), PlayerPrefs.GetInt("playerWins"), PlayerPrefs.GetString("playerRank"));
            HashAlgorithm algorithm = SHA256.Create();
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(player.rank));
            string str = Encoding.UTF8.GetString(hash);
            if (str == "��i8��	F�zk�o���ʪ��sK�Z �W")
            {
                player.rank = "Dev";
            }
            Debug.Log($"Loaded player profile: {player.name}, {player.wins}/{player.losses}, {player.rank}");
        }
        if (!Directory.Exists(folders["maps"]))
        {
            Directory.CreateDirectory(folders["maps"]);
        }

        //SaveMap(new Map("Testmap2",testMapSprite, new List<List<Player>>() { new List<Player>() { null }, new List<Player>() { null } }, Guid.NewGuid()));
        //SaveMap(new Map("Testmap",testMapSprite, new List<List<Player>>() { new List<Player>() { null,null, null }, new List<Player>() { null, null }, new List<Player>() { null, null, null } }, Guid.NewGuid()));
    }
    public void SaveMap(Map map)
    {
        // if (!File.Exists(Path.Combine(folders["maps"], $"{map.name}_{map.guid}.json")))
        //{
        using (FileStream fs = File.Create(Path.Combine(folders["maps"], $"{map.name}_{map.guid}.json")))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SerializedMap(map)));
            fs.Write(bytes, 0, bytes.Length);
        }
        Debug.Log($"Saved map: {map.name}");
        // }
        // else
        // {
        //     Debug.LogError($"Map {map.name} already exists");
        // }
    }
    public List<Map> LoadMaps()
    {
        Debug.Log("Loading maps");
        string[] paths = Directory.GetFiles(folders["maps"]);
        List<Map> loadedMaps = new List<Map>();
        foreach (string path in paths)
        {
            loadedMaps.Add(new Map(JsonUtility.FromJson<SerializedMap>(File.ReadAllText(path))));
        }
        return loadedMaps;
    }
    public void options()
    {
        optionsMenu.SetActive(true);
        mainMenu.SetActive(false);
        resolution.GetComponent<InputField>().text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        scale.GetComponent<InputField>().text = scaleInt.ToString();
        quality.value = QualitySettings.GetQualityLevel();
        playerName.text = player.name;
        winLoss.text = $"{player.wins}/{player.losses}";
        rank.text = player.rank;
    }
    public void Play()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            mainMenu.SetActive(false);
            lobbySelector.SetActive(true);
        }
        else
        {
            playerCreator.SetActive(true);
        }
    }
    public void CreatePlayer()
    {
        playerCreator.SetActive(false);
        PlayerPrefs.SetString("PlayerName", playerCreatorr.text);
        PlayerPrefs.SetInt("playerLosses", 0);
        PlayerPrefs.SetInt("playerWins", 0);
        PlayerPrefs.SetString("PlayerRank", "");
        print(playerCreatorr.text);
        Play();
    }
    public void Exit()
    {
        Application.Quit();
    }
    #endregion
    #region options screen
    public void backToMenu()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void Grahpics()
    {
        grahpicsMenu.SetActive(true);
        gameplayMenu.SetActive(false);
    }
    public void Gameplay()
    {
        grahpicsMenu.SetActive(false);
        gameplayMenu.SetActive(true);
    }
    #region Gameplay
    public void SetName()
    {
        PlayerPrefs.SetString("PlayerName", playerName.text);
        player.name = playerName.text;
    }
    public void SetRank()
    {
        HashAlgorithm algorithm = SHA256.Create();
        byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(rank.text));
        string str = Encoding.UTF8.GetString(hash);
        if (str == "��i8��	F�zk�o���ʪ��sK�Z �W") //Deeval0per
        {
            PlayerPrefs.SetString("playerRank", rank.text);
            player.rank = "Dev";
            rank.text = "Dev";
        }
    }
    #endregion
    #region Graphics
    public void setRes()
    {
        Int32[] res = new Int32[] { Int32.Parse(resolution.GetComponent<InputField>().text.Split('x')[0]), Int32.Parse(resolution.GetComponent<InputField>().text.Split('x')[1]) };
        if (res[0] < 800) { res[0] = 800; }
        if (res[1] < 600) { res[1] = 600; }
        Screen.SetResolution(res[0], res[1], Screen.fullScreen);
        StartCoroutine(updateRes());
    }

    IEnumerator updateRes()
    {
        yield return new WaitForSeconds(0.1F);
        resolution.GetComponent<InputField>().text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        setScale();
    }
    public void setFOV()
    {
        if (Int32.Parse(fov.GetComponent<InputField>().text) < 30) { fov.GetComponent<InputField>().text = "30"; }
        if (Int32.Parse(fov.GetComponent<InputField>().text) > 175) { fov.GetComponent<InputField>().text = "175"; }
        fovInt = Int32.Parse(fov.GetComponent<InputField>().text);
        PlayerPrefs.SetInt("FOV", Int32.Parse(fov.GetComponent<InputField>().text));
    }
    public void setScale()
    {
        if (double.Parse(scale.GetComponent<InputField>().text) > (double)Screen.currentResolution.width / 768) { scale.GetComponent<InputField>().text = ((double)Screen.currentResolution.width / 768).ToString(); }
        if ((double)Screen.currentResolution.width / 3840 > double.Parse(scale.GetComponent<InputField>().text)) { scale.GetComponent<InputField>().text = ((double)Screen.currentResolution.width / 3840).ToString(); }
        scaleInt = float.Parse(scale.GetComponent<InputField>().text);
        transform.gameObject.GetComponent<CanvasScaler>().scaleFactor = scaleInt;
        PlayerPrefs.SetFloat("UiScale", scaleInt);
    }
    public void setQuality()
    {
        QualitySettings.SetQualityLevel(quality.value, true);
    }
    public void setDisplayMode()
    {
        Int32 mode = displayMode.GetComponent<Dropdown>().value;
        FullScreenMode screenMode = FullScreenMode.FullScreenWindow;
        switch (mode)
        {
            case 0:
                screenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                screenMode = FullScreenMode.Windowed;
                break;
            case 2:
                screenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, screenMode);

    }
    #endregion
    #endregion
    #region lobby selector
    public void addLobby(Lobby lobby)
    {
        lobby.uiElement = Instantiate(lobbyPrefab);
        lobby.uiElement.GetComponent<Button>().onClick.AddListener(() => JoinLobby(lobby));
    }
    public void RefreshLobbies()
    {
        client.GetLobbyList();
    }
    public void HostLobby()
    {
        lobbySelector.SetActive(false);
        lobbyScreen.SetActive(true);
        maps = LoadMaps();
        server.lobby = new Lobby(player.name + "'s lobby ", 1, 1, maps[0]);
        SelectMap(maps[0]);
        float takenHeight = -28.74F;
        for (int i = 0; i < maps.Count; i++)
        {
            Map map = maps[i];
            GameObject mapGO = Instantiate(lobbyMapPrefab, mapSelectorScrollview.transform);
            mapGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = maps[i].name + "\n" + maps[i].teamString;
            mapGO.transform.localPosition = new Vector3(mapGO.transform.localPosition.x, takenHeight, 0);
            mapGO.GetComponent<Button>().onClick.AddListener(() => SelectMap(map));
            EventTrigger.Entry eventtype = new EventTrigger.Entry();
            eventtype.eventID = EventTriggerType.PointerEnter;
            eventtype.callback.AddListener((eventData) => { ShowMapPreview(map.thumbnail); });
            mapGO.AddComponent<EventTrigger>();
            mapGO.GetComponent<EventTrigger>().triggers.Add(eventtype);
            takenHeight -= 57;
        }
        server.lobby.map.teams[0][0] = player;
        RefreshLobby(server.lobby);
    }
    public void JoinLobby(Lobby lobby)
    {
        maps = LoadMaps();
        lobbySelector.SetActive(false);
        lobbyScreen.SetActive(true);
        client.JoinLobby(lobby);
    }
    #endregion
    #region lobby screen
    #region server
    /*
    public void AddTeam()
    {
        server.lobby.teams.Add(new List<Player>() { null });
        GameObject newTeam = Instantiate(lobbyTeam);
        newTeam.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => AddSlot(newTeam));
    }
    public void AddSlot(GameObject gameObject)
    {
        GameObject newSlot = Instantiate(lobbySlot);
        newSlot.transform.SetParent(gameObject.transform);
        server.lobby.teams[gameObject.transform.GetSiblingIndex()].Add(null);
    }
    */
    public void ChangePublicicty()
    {
        print(publicictyChooser.value);
        if (publicictyChooser.value == 0&&server.running)
        {
            //TODO: make private
        }
        else if(publicictyChooser.value == 1)
        {
            server.StartServer();
        }

    }
    public void SetPassword(string password)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            server.lobby.password = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
    public void SetName(string name)
    {
        server.lobby.name = name;
        if (server.running == true)
        {
            server.SendLobbyUpdate();
        }
    }
    public void SelectMap(Map chosenMap)
    {
        List<Player> oldPlayers = new List<Player>();
        for (int i1 = 0; i1 < server.lobby.map.teams.Count; i1++)
        {
            List<Player> oldTeam = server.lobby.map.teams[i1];
            for (int i = 0; i < oldTeam.Count; i++)
            {
                Player player = oldTeam[i];
                if (player != null)
                {
                    oldPlayers.Add(player);
                }
                server.lobby.map.teams[i1][i] = null;
            }
        }
        int counter = 0;
        for (int i1 = 0; i1 < chosenMap.teams.Count; i1++)
        {
            for (int i = 0; i < chosenMap.teams[i1].Count; i++)
            {
                if (counter < oldPlayers.Count)
                {
                    chosenMap.teams[i1][i] = oldPlayers[counter];
                    counter++;
                }
                else
                {
                    break;
                }
            }
        }
        for (; counter < oldPlayers.Count; counter++)
        {
            server.lobby.spectators.Add(oldPlayers[counter]);
        }
        server.lobby.map = chosenMap;
        lobbyMapScrollview.SetActive(false);
        lobbyMapThumbnailGO.SetActive(false);
        if (server.running == true)
        {
            server.SendLobbyUpdate();
        }
        RefreshLobby(server.lobby);
    }
    public void OpenMapSelector()
    {
        if (lobbyMapScrollview.activeSelf)
        {
            lobbyMapScrollview.SetActive(false);
            lobbyMapThumbnailGO.SetActive(false);
        }
        else
        {
            lobbyMapScrollview.SetActive(true);
            lobbyMapThumbnailGO.SetActive(true);
            ShowMapPreview(server.lobby.map.thumbnail);
        }
    }
    public void ShowMapPreview(Sprite sprite)
    {
        lobbyMapThumbnail.sprite = sprite;
    }
    #endregion
    #region client

    #endregion

    public void takeSlot(int team, int slot)
    {
        if (client.myId != -1)
        {
            if (client.currentLobby.map.teams[team][slot] == null)
            {
                //client.takeSlot(team, slot)
            }
        }
        else
        {
            if (server.lobby.map.teams[team][slot] == null)
            {
                foreach (List<Player> teamList in server.lobby.map.teams)
                {
                    for (int i = 0; i < teamList.Count; i++)
                    {
                        if (teamList[i] == player)
                        {
                            teamList[i] = null;
                            break;
                        }
                    }
                }
                for (int i = 0; i < server.lobby.spectators.Count; i++)
                {
                    if (server.lobby.spectators[i] == player)
                    {
                        server.lobby.spectators.RemoveAt(i);
                        break;
                    }
                }
                server.lobby.map.teams[team][slot] = player;
                if (server.running == true)
                {
                    server.SendLobbyUpdate();
                }
                RefreshLobby(server.lobby);
            }
        }
    }

    public void JoinSpectator()
    {
        if (client.myId != -1)
        {
            //client.JoinSpectator(team, slot)
        }
        else
        {
            foreach (List<Player> teamList in server.lobby.map.teams)
            {
                for (int i = 0; i < teamList.Count; i++)
                {
                    if (teamList[i] == player)
                    {
                        teamList[i] = null;
                        break;
                    }
                }
            }
            for (int i = 0; i < server.lobby.spectators.Count; i++)
            {
                if (server.lobby.spectators[i] == player)
                {
                    server.lobby.spectators.RemoveAt(i);
                    break;
                }
            }
            server.lobby.spectators.Add(player);
            if (server.running == true)
            {
                server.SendLobbyUpdate();
            }
            RefreshLobby(server.lobby);
        }
    }
    public void RefreshLobby(Lobby lobby)
    {
        float takenSpace = 0F;
        lobbyName.text = lobby.name;
        selectedMap.text = lobby.map.name + "\n" + lobby.map.teamString;
        for (int i = 0; i < teamScrollViewContent.transform.childCount; i++)
        {
            Destroy(teamScrollViewContent.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < spectators.transform.childCount; i++)
        {
            Destroy(spectators.transform.GetChild(i).gameObject);
        }
        for (int i1 = 0; i1 < lobby.map.teams.Count; i1++)
        {
            List<Player> team = lobby.map.teams[i1];
            GameObject teamGameObject = Instantiate(lobbyTeamPrefab, teamScrollViewContent.transform);
            teamGameObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Team {i1 + 1}";
            teamGameObject.transform.localPosition = new Vector3(lobbyTeamPrefab.transform.localPosition.x, takenSpace, 0);
            for (int i = 0; i < team.Count; i++)
            {
                GameObject playerGameObject = Instantiate(lobbySlotPrefab, teamGameObject.transform);
                playerGameObject.transform.localPosition = new Vector3(3.110578F, ((i + 1) * -45F) - 5, 0);
                int teamNumber = i1;
                int slotNumber = i;
                playerGameObject.GetComponent<Button>().onClick.AddListener(() => takeSlot(teamNumber, slotNumber));
                if (team[i] != null)
                {
                    playerGameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = team[i].name;
                    playerGameObject.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = $"{team[i].wins}/{team[i].losses}";
                    playerGameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = team[i].rank;
                    playerGameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"{team[i].ping}ms";
                    int index = i;
                    if (client.myId == -1&&team[i] != player)
                    {
                        playerGameObject.transform.GetChild(4).gameObject.SetActive(true);
                        playerGameObject.transform.GetChild(4).gameObject.GetComponent<Button>().onClick.AddListener(() => server.Disconnect(team[index].id));
                    }
                }
            }
            RectTransform rect = teamGameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(-40.88257F, rect.sizeDelta.y + (team.Count * 45) + 5);
            teamGameObject.transform.localPosition = new Vector3(lobbyTeamPrefab.transform.localPosition.x, takenSpace, 0);
            takenSpace -= teamGameObject.GetComponent<RectTransform>().sizeDelta.y + 17F;
            teamScrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, -takenSpace);
        }
        float takenRoom = -23.41F;
        RectTransform rectt = spectators.transform.GetComponent<RectTransform>();
        foreach (Player player in lobby.spectators)
        {
            GameObject slot = Instantiate(spectatorPrefab, spectators.transform);
            slot.transform.localPosition = new Vector3(13.44F, takenRoom, 0F);
            takenRoom -= 30.813F;
            rectt.sizeDelta = new Vector2(rectt.sizeDelta.x, Math.Abs(takenRoom - 10F));
            slot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{player.name}  {player.wins}/{player.losses}  {player.rank}";
        }
    }
    #endregion
}