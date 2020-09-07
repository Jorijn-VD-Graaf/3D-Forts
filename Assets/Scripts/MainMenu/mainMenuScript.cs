using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Video;
using TMPro;

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

    #region start screen
    public void Start()
    {
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
        List<List<Player>> teams = new List<List<Player>>();
        List<Player> team = new List<Player>();
        List<Player> team2 = new List<Player>();
        List<Player> team3 = new List<Player>();
        team.Add(null);
        team.Add(null);
        team.Add(null);
        team2.Add(null);
        team2.Add(null);
        team3.Add(null);
        team3.Add(null);
        team3.Add(null);
        teams.Add(team);
        teams.Add(team2);
        teams.Add(team3);
        maps.Add(new Map("Testmap","1v1v1",testMapSprite,teams));
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
        if(str == "��i8��	F�zk�o���ʪ��sK�Z �W") //Deeval0per
        {
            PlayerPrefs.SetString("playerRank",rank.text);
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
        server.lobby = new Lobby(player.name + "'s lobby ", 1, 1, maps[0]);
        SelectMap(0);
    }
    public void JoinLobby(Lobby lobby)
    {
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
    public void SetPassword(string password)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            server.lobby.password = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
    public void SetName(string name)
    {
        server.lobby.name = name;
    }
    public void SelectMap(int chosenMap)
    {
        server.lobby.map = maps[chosenMap];
        selectedMap.text = maps[chosenMap].name + "\n" + maps[chosenMap].teamString;
        lobbyMapScrollview.SetActive(false);
        lobbyMapThumbnailGO.SetActive(false);
        RefreshLobby(server.lobby);
    }
    public void OpenMapSelector()
    {
        lobbyMapScrollview.SetActive(true);
        lobbyMapThumbnailGO.SetActive(true);
        float takenHeight = 121.26F;
        for (int i = 0; i < maps.Count; i++)
        {
            Map map = maps[i];
            GameObject mapGO = Instantiate(lobbyMapPrefab);
            mapGO.transform.localPosition.Set(mapGO.transform.localPosition.x, takenHeight, 0);
            mapGO.GetComponent<Button>().onClick.AddListener(() => SelectMap(i));
            takenHeight -= 57;
        }
    }
    #endregion
    #region client

    #endregion
    public void RefreshLobby(Lobby lobby)
    {
        bool first = true;
        float takenSpace = 0;
        lobbyName.text = lobby.name;
        selectedMap.text = lobby.map.name + "\n" + lobby.map.teamString;
        foreach (List<Player> team in lobby.teams)
        {
            GameObject teamGameObject = Instantiate(lobbyTeamPrefab);
            teamGameObject.transform.SetParent(teamScrollViewContent.transform);
            if (first == false)
            {
                teamGameObject.transform.localPosition = new Vector3(lobbyTeamPrefab.transform.localPosition.x, takenSpace, 0);
            }
            else
            {
                first = false;
                teamGameObject.transform.localPosition = lobbyTeamPrefab.transform.localPosition;

            }
            for (int i = 0; i < team.Count; i++)
            {
                RectTransform rect = teamGameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(-40.88257F, rect.sizeDelta.y + 69.817F);
                GameObject playerGameObject = Instantiate(lobbySlotPrefab);
                playerGameObject.transform.SetParent(teamGameObject.transform);
                playerGameObject.transform.localPosition = new Vector3(0, (i+1)* 69.817F, 0);
            }
            teamGameObject.transform.localPosition = new Vector3(lobbyTeamPrefab.transform.localPosition.x, takenSpace, 0);
            takenSpace += teamGameObject.transform.localPosition.y - teamGameObject.GetComponent<RectTransform>().sizeDelta.y - 17.283F;
        }
    }
    #endregion
}